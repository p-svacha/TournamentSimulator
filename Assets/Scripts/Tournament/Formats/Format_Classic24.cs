using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Format_Classic24 : Tournament
{
    protected List<int> PointDistribution_Group = new List<int>() { 10, 6, 4, 3, 2, 1 };
    protected List<int> PointDistribution_KO = new List<int>() { 10, 6, 4, 3 };

    public Format_Classic24(TournamentData data) : base(data) { }
    public Format_Classic24(TournamentSimulator sim, LeagueType type, int season, int quarter, int day, List<Player> players, List<League> allLeagues) : base(sim, type, season, quarter, day, players, allLeagues) { }

    public override void Initialize()
    {
        if (Players.Count != 24) throw new System.Exception("Classic24 tournament must have exactly 24 players and not " + Players.Count);

        PlayersPerPhase = new int[] { 6, 4, 4, 4 };
        MatchesPerPhase = new int[] { 4, 4, 2, 1 };

        List<Player> unassignedPlayers = new List<Player>();
        unassignedPlayers.AddRange(Players);
        Matches = new List<Match>();
        // Group stage
        for (int i = 0; i < 4; i++)
        {
            Match groupMatch = new Match(Simulator, "Group " + (i + 1), this, numPlayers: 6, PointDistribution_Group);

            // Add initial players
            for (int j = 0; j < 6; j++)
            {
                Player p = unassignedPlayers[Random.Range(0, unassignedPlayers.Count)];
                unassignedPlayers.Remove(p);
                groupMatch.AddPlayerToMatch(p);
            }

            Matches.Add(groupMatch);
        }
        // Quarters
        for (int i = 0; i < 4; i++)
        {
            Matches.Add(new Match(Simulator, "Quarterfinal " + (i + 1), this, numPlayers: 4, PointDistribution_KO));
        }
        // Semis
        for (int i = 0; i < 2; i++)
        {
            Matches.Add(new Match(Simulator, "Semifinal " + (i + 1), this, numPlayers: 4, PointDistribution_KO));
        }
        // Final
        Matches.Add(new Match(Simulator, "Final", this, numPlayers: 4, PointDistribution_KO));

        // Link matches
        Matches[0].SetTargetMatches(new List<int>() { 4, 5, 6, 7 });
        Matches[1].SetTargetMatches(new List<int>() { 5, 6, 7, 4 });
        Matches[2].SetTargetMatches(new List<int>() { 6, 7, 4, 5 });
        Matches[3].SetTargetMatches(new List<int>() { 7, 4, 5, 6 });

        Matches[4].SetTargetMatches(new List<int>() { 8, 9 });
        Matches[5].SetTargetMatches(new List<int>() { 9, 8 });
        Matches[6].SetTargetMatches(new List<int>() { 8, 9 });
        Matches[7].SetTargetMatches(new List<int>() { 9, 8 });

        Matches[8].SetTargetMatches(new List<int>() { 10, 10 });
        Matches[9].SetTargetMatches(new List<int>() { 10, 10 });
    }

    protected override void DistributeLeaguePoints()
    {
        // Quarters
        for (int i = 4; i < 8; i++)
        {
            League.Standings[Matches[i].PlayerRanking[2]] += 1;
            League.Standings[Matches[i].PlayerRanking[3]] += 1;
        }
        // Semis
        for (int i = 8; i < 10; i++)
        {
            League.Standings[Matches[i].PlayerRanking[2]] += 2;
            League.Standings[Matches[i].PlayerRanking[3]] += 2;
        }
        // Final
        League.Standings[Matches[10].PlayerRanking[3]] += 3;
        League.Standings[Matches[10].PlayerRanking[2]] += 4;
        League.Standings[Matches[10].PlayerRanking[1]] += 5;
        League.Standings[Matches[10].PlayerRanking[0]] += 6;
    }

    public override List<UI_Group> DisplayTournament(UI_Base baseUI, GameObject Container, UI_Group groupPrefab)
    {
        PlayersPerPhase = new int[] { 6, 6, 4, 4 };
        MatchesPerPhase = new int[] { 4, 4, 2, 1 };
        return base.DisplayTournament(baseUI, Container, groupPrefab);
    }

}