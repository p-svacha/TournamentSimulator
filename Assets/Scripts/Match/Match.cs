using System.Collections;
using System.Collections.Generic;
using System.Linq;
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


    // Rules
    public int NumPlayers { get; private set; } // How many players there are in the match
    public List<int> TargetMatchIndices { get; private set; } // Indices of matches within tournament that participants in this match are advancing to
    public int NumAdvancements => TargetMatchIndices == null ? 0 : TargetMatchIndices.Count;
    public List<int> TargetMatchSeeds { get; private set; } // List containing the seeds that advancing participants will have in their next match. If empty the seed will be the rank in this match.
    public List<int> PointDistribution { get; private set; } // How the round points are distributed among the players based on ranks. Usually {1,0} in 2 player matches, or something like {10,6,4,3} else.

    // State
    public List<Game> Games { get; protected set; }
    public bool IsDone { get; protected set; }
    public bool IsRunning { get; protected set; }
    public List<MatchParticipant_Player> PlayerParticipants { get; private set; }
    public MatchParticipant_Player GetParticipant(Player p) => PlayerParticipants.First(x => x.Player == p);
    

    #region Init / Before start

    // Create a new match with all attributes that are known from the start
    protected Match(string name, Tournament tournament, int quarter, int day, MatchFormatDef format, int numPlayers, List<int> pointDistribution, TournamentGroup group = null)
    {
        Id = Database.GetNewMatchId();
        Name = name;
        Tournament = tournament;
        Quarter = quarter;
        Day = day;
        Format = format;
        NumPlayers = numPlayers;
        PointDistribution = pointDistribution;
        Group = group;

        PlayerParticipants = new List<MatchParticipant_Player>();
        Games = new List<Game>();
        TargetMatchSeeds = new List<int>();
    }

    public void AddPlayerToMatch(Player p, int seed = 0, Team team = null)
    {
        if (IsDone) throw new System.Exception("Cannot add a player to match that is already done.");
        if (PlayerParticipants.Count >= NumPlayers) throw new System.Exception("Can't add a player to a match that is already full. (match has " + PlayerParticipants.Count + "/" + NumPlayers + " players)");
        if (PlayerParticipants.Any(x => x.Player == p)) throw new System.Exception("Can't add the same player to the match twice (" + p.Name + ")");

        PlayerParticipants.Add(new MatchParticipant_Player(p, seed, team));
    }

    public bool CanStartNextGame()
    {
        if (IsDone) return false;
        if (!IsRunning && !CanStartMatch()) return false;
        if (!Games.Last().IsDone) return false;
        if (Games.Last().IsRunning) return false;
        return true;
    }

    /// <summary>
    /// Creates the next game for this match
    /// </summary>
    private Game CreateNextGame()
    {
        int numExistingGames = Games.Count;

        Game nextGame = CreateGame(numExistingGames);
        Games.Add(nextGame);
        return nextGame;
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

    protected abstract Game CreateGame(int index);

    /// <summary>
    /// Sets the matches the top x players in this match advance to, with x being the length of the list and the integers in the list corresponding to the match index within the tournament.
    /// </summary>
    public void SetTargetMatches(List<int> targetMatchIndices)
    {
        TargetMatchIndices = targetMatchIndices;
    }

    public void SetTargetMatchSeeds(List<int> targetMatchSeeds)
    {
        TargetMatchSeeds = targetMatchSeeds;
    }

    public virtual bool CanStartMatch()
    {
        if (IsDone) return false; // match already done
        if (IsRunning) return false; // match already running
        if (NumPlayers != PlayerParticipants.Count) return false; // match not full
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

        // Check if match is over (based on format)
        if(IsMatchOver())
        {
            SetDone();
        }

        // Save game to database and save game state
        Database.AddGame(game);
        TournamentSimulator.Instance.Save();
    }

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
            p.SetElo(newElo);
        }
    }

    #endregion

    #region After Match

    private Dictionary<Player, int> GetNewPlayerEloRatings()
    {
        List<Player> ranking = PlayerRanking;

        Dictionary<Player, int> newEloRatings = new Dictionary<Player, int>();
        foreach (Player p in ranking) newEloRatings.Add(p, p.Elo);

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
        float expWinner = 1f / (1f + Mathf.Pow(10f, (loser.Elo - winner.Elo) / 400f));
        float expLoser = 1f / (1f + Mathf.Pow(10f, (winner.Elo - loser.Elo) / 400f));

        newRatings[winner] += (int)(20 * (1 - expWinner));
        newRatings[loser] += (int)(20 * (0 - expLoser));
    }

    /// <summary>
    /// Returns an a list of all match participants ordered by match result.
    /// </summary>
    public List<MatchParticipant_Player> PlayerParticipantRanking 
    {
        get
        {
            if (IsDone || IsRunning) return PlayerParticipants.OrderByDescending(x => x.MatchScore).ThenByDescending(x => x.Player.TiebreakerScore).ToList();
            else return PlayerSeeding;
        }
    }
    public virtual List<MatchParticipant_Player> PlayerSeeding
    {
        get
        {
            if (Tournament.League != null) return PlayerParticipants.OrderBy(x => x.Seed).ThenByDescending(x => Tournament.League.Standings[x.Player]).ThenByDescending(x => x.Player.Elo).ThenByDescending(x => x.Player.TiebreakerScore).ToList();
            else return PlayerParticipants.OrderBy(x => x.Seed).ThenByDescending(x => x.Player.Elo).ThenByDescending(x => x.Player.TiebreakerScore).ToList();
        }
    }

    /// <summary>
    /// The amount of participating parties that are ranked. Equals the amount of players in a free for all match or the amount of teams in a team match.
    /// </summary>
    public abstract int NumParticipants { get; }

    public List<Player> PlayerRanking => PlayerParticipantRanking.Select(x => x.Player).ToList();
    
    public override string ToString() => Tournament.ToString() + " " + Name + " (" + Id + ")";

    #endregion

    #region Getters

    public Team GetTeamOf(Player p) => GetParticipant(p).Team;

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
        data.NumPlayers = NumPlayers;
        data.TargetMatchIndices = TargetMatchIndices;
        data.TargetMatchSeeds = TargetMatchSeeds;
        data.PointDistribution = PointDistribution;
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
        NumPlayers = data.NumPlayers;
        TargetMatchIndices = data.TargetMatchIndices;
        TargetMatchSeeds = data.TargetMatchSeeds;
        PointDistribution = data.PointDistribution;
        IsDone = data.IsDone;
        PlayerParticipants = data.Participants.Select(x => new MatchParticipant_Player(x)).ToList();
        Games = new List<Game>();

        // References
        Tournament.Matches.Add(this);
        foreach (var p in PlayerParticipants) p.Player.Matches.Add(this);
        if (Group != null) Group.Matches.Add(this);
    }

    #endregion
}
