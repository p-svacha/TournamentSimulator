using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Match
{
    // General
    public int Id { get; private set; }
    public string Name { get; private set; }
    public MatchType Type { get; protected set; }
    public Tournament Tournament { get; private set; }
    public int Quarter { get; private set; }
    public int Day { get; private set; }
    public int AbsoluteDay => Database.ToAbsoluteDay(Tournament.Season, Quarter, Day);
    public int Season => Tournament.Season;
    public string DateString => Database.GetDateString(Season, Quarter, Day);


    // Rules
    public int NumPlayers { get; private set; } // How many players there are in the match
    public List<int> TargetMatchIndices { get; private set; } // Indices of matches within tournament that players in this match are advancing to
    public int NumAdvancements => TargetMatchIndices == null ? 0 : TargetMatchIndices.Count;
    public List<int> PointDistribution { get; private set; } // How the round points are distributed among the players based on ranks

    // State
    public bool IsDone { get; protected set; }
    public bool IsRunning { get; protected set; }
    public List<MatchParticipant> Participants { get; private set; }
    public List<MatchRound> Rounds { get; private set; }

    #region Init / Before start

    // Create a new match with all attributes that are known from the start
    public Match(string name, Tournament tournament, int quarter, int day, int numPlayers, List<int> pointDistribution)
    {
        Id = Database.GetNewMatchId();
        Name = name;
        Type = MatchType.FreeForAll;
        Tournament = tournament;
        Quarter = quarter;
        Day = day;
        NumPlayers = numPlayers;
        PointDistribution = pointDistribution;

        Participants = new List<MatchParticipant>();
        Rounds = new List<MatchRound>();
    }

    public void AddPlayerToMatch(Player p, int seed, Team team = null)
    {
        if (IsDone) throw new System.Exception("Cannot add a player to match that is already done.");
        if (Participants.Count >= NumPlayers) throw new System.Exception("Can't add a player to a match that is already full. (match has " + Participants.Count + "/" + NumPlayers + " players)");
        if (Participants.Any(x => x.Player == p)) throw new System.Exception("Can't add the same player to the match twice (" + p.Name + ")");

        Participants.Add(new MatchParticipant(p, seed, team));
    }

    /// <summary>
    /// Sets the matches the top x players in this match advance to, with x being the length of the list and the integers in the list corresponding to the match index within the tournament.
    /// </summary>
    public void SetTargetMatches(List<int> targetMatchIndices)
    {
        TargetMatchIndices = targetMatchIndices;
    }

    public virtual bool CanStartMatch()
    {
        if (IsDone) return false; // match already done
        if (IsRunning) return false; // match already running
        if (NumPlayers != Participants.Count) return false; // match not full
        if (!IsMatchToday()) return false; // match not today
        return true;
    }

    public bool IsMatchToday() => Database.Season == Season && Database.Quarter == Quarter && Database.Day == Day;

    public virtual void StartMatch()
    {
        if (!CanStartMatch()) throw new System.Exception("Can't start a match that doesn't fulfill all starting requirements.");

        foreach (MatchParticipant participant in Participants) participant.SetPreMatchStats();
        IsRunning = true;
    }

    #endregion

    #region During Match

    /// <summary>
    /// Executes all the logic behind a round in the match for a certain skill and returns the complete results.
    /// </summary>
    public MatchRound CalculateRoundResult(SkillDef skill)
    {
        // Calculate skill score for each participant
        Dictionary<Player, PlayerMatchRound> roundResults = new Dictionary<Player, PlayerMatchRound>();

        foreach (MatchParticipant p in Participants)
        {
            PlayerMatchRound playerResult = p.Player.GetMatchRoundResult(skill);
            roundResults.Add(p.Player, playerResult);
        }

        // Save all values for the round
        List<Player> attributeRanking = roundResults.OrderBy(x => x.Value.Score).Select(x => x.Key).ToList();
        int lastScore = -1;
        int lastPoints = -1;
        for (int rank = 0; rank < attributeRanking.Count; rank++)
        {
            Player player = attributeRanking[rank];
            int score = roundResults[player].Score;
            int points = PointDistribution[PointDistribution.Count - rank - 1];
            if (score == 0) points = 0;
            else if (score == lastScore) points = lastPoints;
            roundResults[player].SetPointsGained(points);
            lastScore = score;
            lastPoints = points;
        }

        return new MatchRound(this, skill.Id, roundResults.Values.ToList());
    }

    /// <summary>
    /// Adds a match round to the match, saving it as a part of it and increasing the participants total scores according to it.
    /// </summary>
    public virtual void ApplyMatchRound(MatchRound round)
    {
        // Distribute Points
        foreach (MatchParticipant participant in Participants)
            participant.IncreaseTotalScore(round.GetPlayerResult(participant.Player).PointsGained);

        // Save
        Rounds.Add(round);
    }

    /// <summary>
    /// Ends the match, calculates the new elo ratings for all players and moves advancing players to the next matches.
    /// </summary>
    public virtual void SetDone()
    {
        // Set match as done
        if (Rounds.Count == 0) throw new System.Exception("Can't end a match without any rounds.");
        IsDone = true;
        IsRunning = false;

        // Adjust player elos
        AdjustPlayerElos();

        // Set player advancements
        for (int i = 0; i < NumAdvancements; i++)
        {
            int rank = i;
            Player advancingPlayer = PlayerRanking[rank];
            Match targetMatch = Tournament.Matches[TargetMatchIndices[rank]];
            Debug.Log(advancingPlayer.ToString() + " is advancing to " + targetMatch.ToString());
            targetMatch.AddPlayerToMatch(advancingPlayer, seed: rank);
        }

        // Check if tournament is done
        if (Tournament.Matches.All(x => x.IsDone)) Tournament.SetDone();
    }

    protected void AdjustPlayerElos()
    {
        Dictionary<Player, int> newEloRatings = GetNewPlayerEloRatings();
        foreach (KeyValuePair<Player, int> kvp in newEloRatings)
        {
            Player p = kvp.Key;
            int newElo = kvp.Value;

            Participants.First(x => x.Player == p).SetEloAfterMatch(newElo);
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
    public virtual List<MatchParticipant> Ranking 
    {
        get
        {
            if (IsDone || IsRunning) return Participants.OrderByDescending(x => x.TotalScore).ThenByDescending(x => x.Player.TiebreakerScore).ToList();
            else if(Tournament.League != null) return Participants.OrderBy(x => x.Seed).ThenByDescending(x => Tournament.League.Standings[x.Player]).ThenByDescending(x => x.Player.Elo).ThenByDescending(x => x.Player.TiebreakerScore).ToList();
            else return Participants.OrderBy(x => x.Seed).ThenByDescending(x => x.Player.Elo).ThenByDescending(x => x.Player.TiebreakerScore).ToList();
        }
    }
    public List<Player> PlayerRanking => Ranking.Select(x => x.Player).ToList();
    
    public override string ToString() => Tournament.ToString() + " " + Name + " (" + Id + ")";

    #endregion

    public Team GetTeamOf(Player p) => Participants.First(x => x.Player == p).Team;

    #region Save / Load

    public MatchData ToData()
    {
        MatchData data = new MatchData();
        data.Id = Id;
        data.Name = Name;
        data.Type = (int)Type;
        data.TournamentId = Tournament.Id;
        data.Quarter = Quarter;
        data.Day = Day;
        data.IsDone = IsDone;
        data.NumPlayers = NumPlayers;
        data.TargetMatchIndices = TargetMatchIndices;
        data.PointDistribution = PointDistribution;
        data.Participants = Participants.Select(x => x.ToData()).ToList();
        data.Rounds = Rounds.Select(x => x.ToData()).ToList();
        return data;
    }

    public Match(MatchData data)
    {
        Id = data.Id;
        Name = data.Name;
        Type = (MatchType)data.Type;
        Tournament = Database.Tournaments[data.TournamentId];
        Quarter = data.Quarter;
        Day = data.Day;
        NumPlayers = data.NumPlayers;
        TargetMatchIndices = data.TargetMatchIndices;
        PointDistribution = data.PointDistribution;
        IsDone = data.IsDone;
        Participants = data.Participants.Select(x => new MatchParticipant(x)).ToList();
        Rounds = data.Rounds.Select(x => new MatchRound(this, x)).ToList();

        // Parent ref
        Tournament.Matches.Add(this);
    }

    #endregion
}
