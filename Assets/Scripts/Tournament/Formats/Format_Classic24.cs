using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Format_Classic24 : Tournament
{
    protected List<int> PointDistribution_Group = new List<int>() { 10, 6, 4, 3, 2, 1 };
    protected List<int> PointDistribution_KO = new List<int>() { 10, 6, 4, 3 };
    
    public int Quarter { get; private set; }
    public int Day { get; private set; }

    public Format_Classic24(TournamentData data) : base(data) { }
    public Format_Classic24(TournamentType type, int season, int quarter, int day, League league) : base(type, season, league)
    {
        Players = Database.Players.Values.Where(x => x.LeagueType == type).ToList();
        Quarter = quarter;
        Day = day;

        Initialize();
    }

    public override void Initialize()
    {
        if (Players.Count != 24) throw new System.Exception("Classic24 tournament must have exactly 24 players and not " + Players.Count);

        Matches = new List<Match>();

        // Group stage
        List<Player> unassignedPlayers = new List<Player>();
        unassignedPlayers.AddRange(Players);
        for (int i = 0; i < 4; i++)
        {
            Match groupMatch = new Match("Group " + (i + 1), this, Quarter, Day, numPlayers: 6, PointDistribution_Group);

            // Add initial players
            for (int j = 0; j < 6; j++)
            {
                Player p = unassignedPlayers[Random.Range(0, unassignedPlayers.Count)];
                unassignedPlayers.Remove(p);
                groupMatch.AddPlayerToMatch(p, seed: 0);
            }

            Matches.Add(groupMatch);
        }
        // Quarters
        for (int i = 0; i < 4; i++)
        {
            Matches.Add(new Match("Quarterfinal " + (i + 1), this, Quarter, Day, numPlayers: 4, PointDistribution_KO));
        }
        // Semis
        for (int i = 0; i < 2; i++)
        {
            Matches.Add(new Match("Semifinal " + (i + 1), this, Quarter, Day, numPlayers: 4, PointDistribution_KO));
        }
        // Final
        Matches.Add(new Match("Final", this, Quarter, Day, numPlayers: 4, PointDistribution_KO));

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

    protected override void OnTournamentDone()
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
        PlayersPerPhase = new int[] { 6, 4, 4, 4 };
        MatchesPerPhase = new int[] { 4, 4, 2, 1 };
        return base.DisplayTournament(baseUI, Container, groupPrefab);
    }

    public override string GetMatchDayTitle(int index)
    {
        int stepIndex = Database.Tournaments.Values.Where(x => x.League == League).OrderBy(x => x.Matches[0].Quarter).ThenBy(x => x.Matches[0].Day).ToList().IndexOf(this) + 1;
        return "Step " + stepIndex;
    }

}
