using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Match
{
    private TournamentSimulator Simulator;


    // General
    public int Id { get; private set; }
    public string Name { get; private set; }
    public Tournament Tournament { get; private set; }

    // Rules
    public int NumPlayers { get; private set; } // How many players there are in the match
    public List<int> TargetMatchIndices { get; private set; } // Indices of matches within tournament that players in this match are advancing to
    public int NumAdvancements => TargetMatchIndices == null ? 0 : TargetMatchIndices.Count;
    public List<int> PointDistribution { get; private set; }

    // State
    public bool IsDone { get; private set; }
    public List<MatchParticipant> Participants { get; private set; }
    public List<MatchRound> Rounds { get; private set; }

    #region Init / Before start

    // Create a new match with all attributes that are known from the start
    public Match(TournamentSimulator sim, string name, Tournament tournament, int numPlayers, List<int> pointDistribution)
    {
        Simulator = sim;
        Id = Database.GetNewMatchId();
        Name = name;
        Tournament = tournament;
        NumPlayers = numPlayers;
        PointDistribution = pointDistribution;

        Participants = new List<MatchParticipant>();
        Rounds = new List<MatchRound>();
    }

    public void AddPlayerToMatch(Player p)
    {
        if (IsDone) throw new System.Exception("Cannot add a player to match that is already done.");
        if (Participants.Count >= NumPlayers) throw new System.Exception("Can't add a player to a match that is already full. (match has " + Participants.Count + "/" + NumPlayers + " players)");

        Participants.Add(new MatchParticipant(p));
    }

    public void SetTargetMatches(List<int> targetMatchIndices)
    {
        TargetMatchIndices = targetMatchIndices;
    }

    public bool CanSimulate()
    {
        if (IsDone) return false;
        if (NumPlayers != Participants.Count) return false;
        if (Tournament.League.Season != Simulator.Season) return false;
        if (Tournament.Quarter != Simulator.Quarter) return false;
        if (Tournament.Day != Simulator.Day) return false;
        return true;
    }

    #endregion

    #region During Match

    public void AddMatchRound(MatchRound round)
    {
        Rounds.Add(round);
    }

    /// <summary>
    /// Ends the match, calculates the new elo ratings for all players and moves advancing players to the next matches.
    /// </summary>
    public void SetDone()
    {
        if (Rounds.Count == 0) throw new System.Exception("Can't end a match without any rounds.");

        IsDone = true;
        List<Player> orderedPlayers = PlayerRanking;

        Dictionary<Player, int> newEloRatings = new Dictionary<Player, int>();
        foreach (Player p in Participants.Select(x => x.Player)) newEloRatings.Add(p, p.Elo);
        GetNewRatings(orderedPlayers, newEloRatings);
        foreach (KeyValuePair<Player, int> kvp in newEloRatings)
        {
            Player p = kvp.Key;
            int newElo = kvp.Value;

            Participants.First(x => x.Player == p).SetEloAfterMatch(newElo);
            p.SetElo(newElo);
        }

        for (int i = 0; i < NumAdvancements; i++)
        {
            Player advancingPlayer = orderedPlayers[i];
            Match targetMatch = Tournament.Matches[TargetMatchIndices[i]];
            Debug.Log(advancingPlayer.ToString() + " is advancing to " + targetMatch.ToString());
            targetMatch.AddPlayerToMatch(advancingPlayer);
        }

        if (NumAdvancements == 0) Tournament.SetDone();
    }

    #endregion

    #region After Match

    private void GetNewRatings(List<Player> ranking, Dictionary<Player, int> newRatings)
    {
        for(int i = 0; i < ranking.Count; i++)
        {
            for(int j = i + 1; j < ranking.Count; j++)
            {
                GetAdjustedNewRatings(newRatings, ranking[i], ranking[j]);
            }
        }
    }

    private void GetAdjustedNewRatings(Dictionary<Player, int> newRatings, Player winner, Player loser)
    {
        float expWinner = 1f / (1f + Mathf.Pow(10f, (loser.Elo - winner.Elo) / 400f));
        float expLoser = 1f / (1f + Mathf.Pow(10f, (winner.Elo - loser.Elo) / 400f));

        newRatings[winner] += (int)(20 * (1 - expWinner));
        newRatings[loser] += (int)(20 * (0 - expLoser));
    }

    /// <summary>
    /// Returns the results of the match as a player list. So element 0 will be the player that won, etc. etc.
    /// </summary>
    public List<Player> PlayerRanking { get { return GetResult().Select(x => x.Key).ToList(); } }

    /// <summary>
    /// Returns an ordered dictionary with the score of the match for each player.
    /// </summary>
    public Dictionary<Player, int> GetResult()
    {
        return Participants.OrderByDescending(x => x.TotalScore).ToDictionary(x => x.Player, x => x.TotalScore);
    }

    public override string ToString() => Tournament.ToString() + " " + Name + " (" + Id + ")";

    #endregion

    #region Save / Load

    public MatchData ToData()
    {
        MatchData data = new MatchData();
        data.Id = Id;
        data.Name = Name;
        data.TournamentId = Tournament.Id;
        data.IsDone = IsDone;
        data.NumPlayers = NumPlayers;
        data.TargetMatchIndices = TargetMatchIndices;
        data.PointDistribution = PointDistribution;
        data.Participants = Participants.Select(x => x.ToData()).ToList();
        data.Rounds = Rounds.Select(x => x.ToData()).ToList();
        return data;
    }

    public Match(TournamentSimulator sim, MatchData data)
    {
        Simulator = sim;
        Id = data.Id;
        Name = data.Name;
        Tournament = Database.Tournaments[data.TournamentId];
        NumPlayers = data.NumPlayers;
        TargetMatchIndices = data.TargetMatchIndices;
        PointDistribution = data.PointDistribution;
        IsDone = data.IsDone;
        Participants = data.Participants.Select(x => new MatchParticipant(x)).ToList();
        Rounds = data.Rounds.Select(x => new MatchRound(x)).ToList();

        // Parent ref
        Tournament.Matches.Add(this);
    }

    #endregion
}
