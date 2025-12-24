using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor.Search;
using UnityEngine;

public abstract class Match
{
    // General
    public int Id { get; private set; }
    public string Name { get; private set; }
    public MatchFormatDef Format { get; protected set; }
    public Tournament Tournament { get; private set; }
    public TournamentGroup Group { get; private set; }
    public int Quarter { get; private set; }
    public int Day { get; private set; }
    public int AbsoluteDay => Database.ToAbsoluteDay(Tournament.Season, Quarter, Day);
    public int Season => Tournament.Season;
    public string DateString => Database.GetDateString(Season, Quarter, Day);
    public bool IsTeamMatch { get; protected set; }
    public Discipline Discipline => Tournament.Discipline;

    // Classic Settings
    public List<int> PointDistribution { get; private set; } // How the round points are distributed among the players based on ranks. Usually {1,0} in 2 player matches, or something like {10,6,4,3} else.


    // Knockout Settings
    public bool IsKnockout { get; private set; }
    public int KnockoutStartingLives { get; private set; }
    public int KnockoutNumLiveWinners { get; private set; } // First x every round get back a live
    public int KnockoutNumLiveLosers { get; private set; } // Last x every round lose a live


    // Rules
    public int MinPlayers { get; private set; } // How many players must be in the match at minimum for it to be able to start.
    public int MaxPlayers { get; private set; } // How many players can be in the match at maxiumum for it to be able to start.
    public int NumAdvancements => AdvancementsTargets == null ? 0 : AdvancementsTargets.Count;
    /// <summary>
    /// List containing all information about which ranks in this match advance to what matches with what seeds.
    /// </summary>
    public List<MatchAdvancementTarget> AdvancementsTargets { get; private set; } 

    // State
    public List<Game> Games { get; protected set; }
    public bool IsDone { get; protected set; }
    public bool IsRunning { get; protected set; }
    public List<MatchParticipant_Player> PlayerParticipants { get; private set; }
    public MatchParticipant_Player GetParticipant(Player p) => PlayerParticipants.First(x => x.Player == p);
    

    #region Init / Before start

    // Create a new match with all attributes that are known from the start
    protected Match(string name, Tournament tournament, int quarter, int day, MatchFormatDef format, int maxPlayers, List<int> pointDistribution, int minPlayers = -1, TournamentGroup group = null, bool isKnockout = false, int knockoutStartingLives = 0, int koLiveGainers = 0, int koLiveLosers = 0)
    {
        Id = Database.GetNewMatchId();
        Name = name;
        Tournament = tournament;
        Quarter = quarter;
        Day = day;
        Format = format;
        MaxPlayers = maxPlayers;
        MinPlayers = minPlayers == -1 ? maxPlayers : minPlayers; // If min not explicitly set, set it the same as maximum
        PointDistribution = pointDistribution == null ? new List<int>() : pointDistribution;
        Group = group;
        IsKnockout = isKnockout;
        KnockoutStartingLives = knockoutStartingLives;
        KnockoutNumLiveWinners = koLiveGainers;
        KnockoutNumLiveLosers = koLiveLosers;

        PlayerParticipants = new List<MatchParticipant_Player>();
        Games = new List<Game>();
        AdvancementsTargets = new List<MatchAdvancementTarget>();

        if (MinPlayers > MaxPlayers) throw new System.Exception($"minPlayers cannot be higher than maxPlayers. max: {MaxPlayers}, min: {MinPlayers}");
        if (!IsKnockout && PointDistribution.Count == 0) throw new System.Exception("Point distribution must be set in classic mode.");
        if (IsKnockout && KnockoutStartingLives <= 0) throw new System.Exception("KnockoutStartingLives must be greater than 0 in knockout mode.");
        if (IsKnockout && (KnockoutNumLiveWinners >= KnockoutNumLiveLosers)) throw new System.Exception("More players have to lose a life than gain a life in knockout.");
    }

    public void AddPlayerToMatch(Player p, int seed = 0, Team team = null)
    {
        if (IsDone) throw new System.Exception("Cannot add a player to match that is already done.");
        if (PlayerParticipants.Count >= MaxPlayers) throw new System.Exception("Can't add a player to a match that is already full. (match has " + PlayerParticipants.Count + "/" + MaxPlayers + " players)");
        if (PlayerParticipants.Any(x => x.Player == p)) throw new System.Exception("Can't add the same player to the match twice (" + p.Name + ")");

        PlayerParticipants.Add(new MatchParticipant_Player(this, p, seed, team));
        p.AddMatch(this);
    }

    public bool CanStartNextGame()
    {
        if (IsDone) return false;
        if (!IsRunning && !CanStartMatch()) return false;
        if (Games.Count > 0 && !Games.Last().IsDone) return false;
        if (Games.Count > 0 && Games.Last().IsRunning) return false;
        return true;
    }

    /// <summary>
    /// Creates the next game for this match
    /// </summary>
    private Game CreateNextGame()
    {
        int gameIndex = Games.Count;

        List<GameModifierDef> gameModifiers = GenerateGameModifiersFor(gameIndex);

        Game nextGame = CreateGame(gameIndex, gameModifiers);
        Games.Add(nextGame);
        return nextGame;
    }

    private List<GameModifierDef> GenerateGameModifiersFor(int gameIndex)
    {
        List<GameModifierDef> mods = new List<GameModifierDef>();
        mods.AddRange(Tournament.Modifiers);
        return mods;
    }

    /// <summary>
    /// Creates the next game for this match and instantly starts the simulation.
    /// </summary>
    public void SimulateNextGame(float stepTime)
    {
        if (!CanStartNextGame()) throw new System.Exception("Can't start next game.");
        if (!IsRunning) StartMatch();
        Game nextGame = CreateNextGame();
        UI_Base.Instance.StartGameSimulation(nextGame, stepTime);
    }

    protected abstract Game CreateGame(int index, List<GameModifierDef> gameModifiers);

    /// <summary>
    /// Adds advancement targets to this match that define which ranks will move on to which other matches with which seeds.
    /// </summary>
    public void SetAdvancements(List<MatchAdvancementTarget> targets)
    {
        foreach(MatchAdvancementTarget target in targets)
        {
            // Validate
            if (target.SourceMatch != this) throw new System.Exception("Cannot add an advancement target that is not for this match.");
            if (AdvancementsTargets.Any(t => t.SourceRank == target.SourceRank)) throw new System.Exception("An advancement target for the same rank has already been set.");

            // Add
            AdvancementsTargets.Add(target);
        }
    }

    /// <summary>
    /// Short of way of setting advancement target matches for this match.
    /// List contains the LOCAL (within torunament) id of the match, the ranks according to the list index advance to.
    /// Seeds will be equivalent to rank if not set specifically.
    /// </summary>
    public void SetTargetMatches(List<int> targetMatchIds, List<int>? targetMatchSeeds = null)
    {
        for (int i = 0; i < targetMatchIds.Count; i++)
        {
            int seed = i;
            if (targetMatchSeeds != null && i < targetMatchSeeds.Count) seed = targetMatchSeeds[i];
            MatchAdvancementTarget target = new MatchAdvancementTarget(this, i, Tournament.Matches[targetMatchIds[i]], seed);
            AdvancementsTargets.Add(target);
        }
    }

    public virtual bool CanStartMatch()
    {
        if (IsDone) return false; // match already done
        if (IsRunning) return false; // match already running
        if (PlayerParticipants.Count < MinPlayers) return false; // not enough players
        if (PlayerParticipants.Count > MaxPlayers) return false; // too many players
        if (!IsToday) return false; // match not today
        return true;
    }

    public bool IsToday => Database.Season == Season && Database.Quarter == Quarter && Database.Day == Day;

    protected virtual void StartMatch()
    {
        if (!CanStartMatch()) throw new System.Exception("Can't start a match that doesn't fulfill all starting requirements.");

        foreach (MatchParticipant_Player participant in PlayerParticipants) participant.SetPreMatchStats();
        IsRunning = true;
    }

    /// <summary>
    /// Gets executed when the day this match happens is starting.
    /// </summary>
    public virtual void OnDayStart() { }

    #endregion

    #region During Match

    /// <summary>
    /// Gets called when a game in this match is done simulating.
    /// </summary>
    public void OnGameDone(Game game)
    {
        // Validate
        if (!Games.Contains(game)) throw new System.Exception("Can't conclude a game that is not part of this match.");
        if (IsDone) throw new System.Exception("Can't conclude a game on a match that is already done.");
        if (!IsRunning) throw new System.Exception("Can't conclude a game on a match that is not running.");
        if (!game.IsDone) throw new System.Exception("Can't conclude a game that is not done.");
        if (game.IsRunning) throw new System.Exception("Can't conclude a game that is still running.");

        // Adjust skill values of all players
        foreach(MatchParticipant_Player playerParticipant in PlayerParticipants)
        {
            playerParticipant.Player.AdjustEndOfGameSkills();
        }

        // Check if match is over (based on format)
        if (IsMatchOver())
        {
            SetDone();
        }

        // Save game to database and save game state
        Database.AddGame(game);
        TournamentSimulator.Instance.Save();
    }

    /// <summary>
    /// Called after each game in this match finishes and returns if the match as a whole is finished.
    /// </summary>
    private bool IsMatchOver()
    {
        if (Format == MatchFormatDefOf.SingleGame)
        {
            return true;
        }
        throw new System.Exception($"IsMatchOver() not handled for format {Format.DefName}");
    }

    /// <summary>
    /// Ends the match, calculates the new elo ratings for all players and moves advancing players to the next matches.
    /// </summary>
    protected abstract void SetDone();

    protected void MarkMatchAsDone()
    {
        if (Games.Count == 0) throw new System.Exception("Can't end a match without any games.");
        IsDone = true;
        IsRunning = false;
    }

    /// <summary>
    /// Checks if the group or torunament of this match is done if so concludes them.
    /// </summary>
    protected void TryConcludeParents()
    {
        if (Group != null && Group.IsDone) Group.Conclude();
        if (Tournament.Matches.All(x => x.IsDone)) Tournament.SetDone();
    }

    protected void AdjustPlayerElos()
    {
        Dictionary<Player, int> newEloRatings = GetNewPlayerEloRatings();
        foreach (KeyValuePair<Player, int> kvp in newEloRatings)
        {
            Player p = kvp.Key;
            int newElo = kvp.Value;

            GetParticipant(p).SetEloAfterMatch(newElo);
            p.SetElo(Discipline.Def, newElo);
        }
    }

    #endregion

    #region After Match

    private Dictionary<Player, int> GetNewPlayerEloRatings()
    {
        List<Player> ranking = PlayerRanking;

        Dictionary<Player, int> newEloRatings = new Dictionary<Player, int>();
        foreach (Player p in ranking) newEloRatings.Add(p, p.Elo[Discipline.Def]);

        for (int i = 0; i < ranking.Count; i++)
        {
            for (int j = i + 1; j < ranking.Count; j++)
            {
                GetAdjustedNewRatings(newEloRatings, ranking[i], ranking[j]);
            }
        }

        return newEloRatings;
    }

    private void GetAdjustedNewRatings(Dictionary<Player, int> newRatings, Player winner, Player loser)
    {
        float expWinner = 1f / (1f + Mathf.Pow(10f, (loser.Elo[Discipline.Def] - winner.Elo[Discipline.Def]) / 400f));
        float expLoser = 1f / (1f + Mathf.Pow(10f, (winner.Elo[Discipline.Def] - loser.Elo[Discipline.Def]) / 400f));

        newRatings[winner] += (int)(20 * (1 - expWinner));
        newRatings[loser] += (int)(20 * (0 - expLoser));
    }

    /// <summary>
    /// Returns an a list of all match participants ordered by match result.
    /// </summary>
    public List<MatchParticipant_Player> GetPlayerRanking() 
    {
        if (IsDone || IsRunning)
        {
            if (Format == MatchFormatDefOf.SingleGame)
            {
                return new List<MatchParticipant_Player>(Games[0].GetPlayerRanking());
            }
            throw new System.NotImplementedException();
        }
        else return GetPlayerSeeding();
    }
    public int GetPlayerMatchScore(MatchParticipant_Player player)
    {
        if (Format == MatchFormatDefOf.SingleGame)
        {
            return Games[0].GetPlayerPoints(player);
        }
        throw new System.NotImplementedException();
    }
    public int GetPlayerMatchTiebreakerScore(MatchParticipant_Player player)
    {
        if (Format == MatchFormatDefOf.SingleGame)
        {
            return Games[0].GetTotalPlayerScore(player);
        }
        throw new System.NotImplementedException();
    }

    public virtual List<MatchParticipant_Player> GetPlayerSeeding()
    {
        if (Tournament.League != null) return PlayerParticipants.OrderBy(x => x.Seed).ThenByDescending(x => Tournament.League.Standings[x.Player]).ThenByDescending(x => x.Player.Elo[Discipline.Def]).ToList();
        else return PlayerParticipants.OrderBy(x => x.Seed).ThenByDescending(x => x.Player.Elo[Discipline.Def]).ToList();
    }

    /// <summary>
    /// The amount of participating parties that are ranked. Equals the amount of players in a free for all match or the amount of teams in a team match.
    /// </summary>
    public abstract int NumParticipants { get; }

    public int NumPlayerParticipants => PlayerParticipants.Count;

    public List<Player> PlayerRanking => GetPlayerRanking().Select(x => x.Player).ToList();
    public Dictionary<int, List<Player>> GetPlayerRankingWithRanks()
    {
        Dictionary<int, List<Player>> dict = new Dictionary<int, List<Player>>();
        for (int i = 0; i < PlayerRanking.Count; i++)
        {
            dict.Add(i, new List<Player>() { PlayerRanking[i] });
        }
        return dict;
    }

    public override string ToString() => Tournament.ToString() + " " + Name + " (" + Id + ")";

    #endregion

    #region Getters

    /// <summary>
    /// Returns if the given player is a participant of this match.
    /// </summary>
    public bool IsParticipant(Player player) => PlayerParticipants.Any(p => p.Player == player);

    public int GetIdWithinTournament() => Tournament.Matches.IndexOf(this);

    #endregion

    #region Save / Load

    public virtual MatchData ToData()
    {
        MatchData data = new MatchData();
        data.Id = Id;
        data.Name = Name;
        data.IsTeamMatch = IsTeamMatch;
        data.TournamentId = Tournament.Id;
        data.GroupIndex = Group == null ? -1 : Tournament.Groups.IndexOf(Group);
        data.Quarter = Quarter;
        data.Day = Day;
        data.Format = Format.DefName;
        data.IsDone = IsDone;
        data.MinPlayers = MinPlayers;
        data.MaxPlayers = MaxPlayers;
        data.AdvancementTargets = AdvancementsTargets.Select(x => x.ToData()).ToList();
        data.PointDistribution = PointDistribution;
        data.IsKnockout = IsKnockout;
        data.KnockoutStartingLives = KnockoutStartingLives;
        data.KnockoutNumLiveWinners = KnockoutNumLiveWinners;
        data.KnockoutNumLiveLosers = KnockoutNumLiveLosers;
        data.Participants = PlayerParticipants.Select(x => x.ToData()).ToList();
        return data;
    }

    public static Match LoadMatch(MatchData data)
    {
        if (data.IsTeamMatch) return new TeamMatch(data);
        else return new SoloMatch(data);
    }
    protected Match(MatchData data)
    {
        Id = data.Id;
        Name = data.Name;
        IsTeamMatch = data.IsTeamMatch;
        Tournament = Database.GetTournament(data.TournamentId);
        Group = data.GroupIndex == -1 ? null : Tournament.Groups[data.GroupIndex];
        Quarter = data.Quarter;
        Day = data.Day;
        Format = DefDatabase<MatchFormatDef>.GetNamed(data.Format);
        MaxPlayers = data.MaxPlayers;
        MinPlayers = data.MinPlayers;
        AdvancementsTargets = data.AdvancementTargets.Select(x => new MatchAdvancementTarget(this, x)).ToList();
        PointDistribution = data.PointDistribution;
        IsKnockout = data.IsKnockout;
        KnockoutStartingLives = data.KnockoutStartingLives;
        KnockoutNumLiveWinners = data.KnockoutNumLiveWinners;
        KnockoutNumLiveLosers = data.KnockoutNumLiveLosers;
        IsDone = data.IsDone;
        PlayerParticipants = data.Participants.Select(x => new MatchParticipant_Player(this, x)).ToList();
        Games = new List<Game>();

        // References
        Tournament.Matches.Add(this);
        foreach (var p in PlayerParticipants) p.Player.AddMatch(this);
        if (Group != null) Group.Matches.Add(this);
    }

    public void OnLoadingDone()
    {
        foreach (MatchAdvancementTarget advTarget in AdvancementsTargets) advTarget.ResolveReferences();
    }

    #endregion
}
