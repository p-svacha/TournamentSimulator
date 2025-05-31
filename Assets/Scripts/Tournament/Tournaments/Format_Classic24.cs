using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Base format for 24 player leagues.
/// </summary>
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
        if (Teams.Count != 0) throw new System.Exception("Player tournaments cannot have team participants, only players.");

        Groups = new List<TournamentGroup>();
        Matches = new List<Match>();

        // Group stage
        List<Player> unassignedPlayers = new List<Player>();
        unassignedPlayers.AddRange(Players);
        for (int i = 0; i < 4; i++)
        {
            Match groupMatch = new FreeForAllMatch("Group " + (i + 1), this, Quarter, Day, numPlayers: 6, PointDistribution_Group);

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
            Matches.Add(new FreeForAllMatch("Quarterfinal " + (i + 1), this, Quarter, Day, numPlayers: 4, PointDistribution_KO));
        }
        // Semis
        for (int i = 0; i < 2; i++)
        {
            Matches.Add(new FreeForAllMatch("Semifinal " + (i + 1), this, Quarter, Day, numPlayers: 4, PointDistribution_KO));
        }
        // Final
        Matches.Add(new FreeForAllMatch("Final", this, Quarter, Day, numPlayers: 4, PointDistribution_KO));

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

    public override void DisplayTournament(UI_Base baseUI, GameObject Container)
    {
        int[] playersPerPhase = new int[] { 6, 4, 4, 4 };
        int[] matchesPerPhase = new int[] { 4, 4, 2, 1 };
        DisplayTournamentAsLayers(baseUI, Container, playersPerPhase, matchesPerPhase);
    }

    public override string GetMatchDayTitle(int index)
    {
        int stepIndex = Database.Tournaments.Values.Where(x => x.League == League).OrderBy(x => x.Matches[0].Quarter).ThenBy(x => x.Matches[0].Day).ToList().IndexOf(this) + 1;
        return "Step " + stepIndex;
    }

    public override Dictionary<int, List<Player>> PlayerRanking => Matches.Last().PlayerParticipantRanking.ToDictionary(x => Matches.Last().PlayerParticipantRanking.IndexOf(x), x => new List<Player>() { x.Player });
    public override Dictionary<int, List<Team>> TeamRanking => PlayerRanking.ToDictionary(x => x.Key, x => x.Value.Select(x => Database.GetNationalTeam(x.Country)).ToList());

}
