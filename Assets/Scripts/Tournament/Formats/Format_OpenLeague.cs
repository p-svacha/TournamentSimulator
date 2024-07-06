using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Format_OpenLeague : Tournament
{
    protected List<int> PointDistribution_KO = new List<int>() { 10, 6, 4, 3 };

    private Dictionary<int, List<int>> PointDistributions = new Dictionary<int, List<int>>()
    {
        { 1, new List<int>() { 10 } },
        { 2, new List<int>() { 10, 6 } },
        { 3, new List<int>() { 10, 6, 4} },
        { 4, new List<int>() { 10, 6, 4, 3} },
        { 5, new List<int>() { 10, 6, 4, 3, 2} },
        { 6, new List<int>() { 10, 6, 4, 3, 2, 1} },
        { 7, new List<int>() { 10, 8, 6, 4, 3, 2, 1} },
        { 8, new List<int>() { 10, 8, 6, 5, 4, 3, 2, 1} },
        { 9, new List<int>() { 10, 8, 7, 6, 5, 4, 3, 2, 1} },
        { 10, new List<int>() { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1} },
        { 11, new List<int>() { 12, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1} },
        { 12, new List<int>() { 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1} },
    };

    public int Quarter { get; private set; }
    public int Day { get; private set; }

    public Format_OpenLeague(TournamentData data) : base(data) { }
    public Format_OpenLeague(int season, int quarter, int day, League league) : base(TournamentType.OpenLeague, season, league)
    {
        Players = Database.Players.Values.Where(x => x.LeagueType == TournamentType.OpenLeague).ToList();
        Quarter = quarter;
        Day = day;

        Initialize();
    }

    public override void Initialize()
    {
        if (Players.Count > 48) throw new System.Exception("Open League does not support more than 48 players");
        if (Teams.Count != 0) throw new System.Exception("Player tournaments cannot have team participants, only players.");
        Name = "Open League";

        List<Player> unassignedPlayers = new List<Player>();
        unassignedPlayers.AddRange(Players);
        Groups = new List<TournamentGroup>();
        Matches = new List<Match>();

        if (Players.Count == 0) IsDone = true;
        else if(Players.Count < 6) // Only 1 match for 1-5 players
        {
            Match onlyMatch = new Match("Final", this, Quarter, Day, numPlayers: Players.Count, PointDistributions[Players.Count]);
            foreach (Player p in unassignedPlayers) onlyMatch.AddPlayerToMatch(p, seed: 0);
            Matches.Add(onlyMatch);
        }

        else if(Players.Count < 12) // Semifinals + Final with 6-11 players
        {
            List<List<Player>> groups = new List<List<Player>>();
            int numGroups = 2;
            for (int i = 0; i < numGroups; i++) groups.Add(new List<Player>());
            int curGroup = 0;
            while(unassignedPlayers.Count > 0)
            {
                Player groupPlayer = unassignedPlayers[Random.Range(0, unassignedPlayers.Count)];
                unassignedPlayers.Remove(groupPlayer);
                groups[curGroup].Add(groupPlayer);
                curGroup++;
                if (curGroup >= numGroups) curGroup = 0;
            }
            // Semis
            for (int i = 0; i < 2; i++)
            {
                Match semi = new Match("Semifinal " + (i + 1), this, Quarter, Day, numPlayers: groups[i].Count, PointDistributions[groups[i].Count]);
                foreach (Player p in groups[i]) semi.AddPlayerToMatch(p, seed: 0);
                Matches.Add(semi);
            }
            // Final
            Matches.Add(new Match("Final", this, Quarter, Day, numPlayers: 4, PointDistribution_KO));

            // Link matches
            Matches[0].SetTargetMatches(new List<int>() { 2, 2 });
            Matches[1].SetTargetMatches(new List<int>() { 2, 2 });
        }

        else if(Players.Count < 24) // Quarters + Semifinals + Final with 12-23 players
        {
            List<List<Player>> groups = new List<List<Player>>();
            int numGroups = 4;
            for (int i = 0; i < numGroups; i++) groups.Add(new List<Player>());
            int curGroup = 0;
            while (unassignedPlayers.Count > 0)
            {
                Player groupPlayer = unassignedPlayers[Random.Range(0, unassignedPlayers.Count)];
                unassignedPlayers.Remove(groupPlayer);
                groups[curGroup].Add(groupPlayer);
                curGroup++;
                if (curGroup >= numGroups) curGroup = 0;
            }

            // Quarters
            for (int i = 0; i < 4; i++)
            {
                Match quarter = new Match("Quarterfinal " + (i + 1), this, Quarter, Day, numPlayers: groups[i].Count, PointDistributions[groups[i].Count]);
                foreach (Player p in groups[i]) quarter.AddPlayerToMatch(p, seed: 0);
                Matches.Add(quarter);
            }
            // Semis
            for (int i = 0; i < 2; i++)
            {
                Matches.Add(new Match("Semifinal " + (i + 1), this, Quarter, Day, numPlayers: 4, PointDistribution_KO));
            }
            // Final
            Matches.Add(new Match("Final", this, Quarter, Day, numPlayers: 4, PointDistribution_KO));

            // Link matches
            Matches[0].SetTargetMatches(new List<int>() { 4, 5 });
            Matches[1].SetTargetMatches(new List<int>() { 5, 4 });
            Matches[2].SetTargetMatches(new List<int>() { 4, 5 });
            Matches[3].SetTargetMatches(new List<int>() { 5, 4 });

            Matches[4].SetTargetMatches(new List<int>() { 6, 6 });
            Matches[5].SetTargetMatches(new List<int>() { 6, 6 });
        }

        else // Groups + Quarters + Semifinals + Final with 24+ players
        {
            int maxGroupSize = Players.Count / 4;
            List<List<Player>> groups = new List<List<Player>>();
            int numGroups = 4;
            for (int i = 0; i < numGroups; i++) groups.Add(new List<Player>());
            int curGroup = 0;
            while (unassignedPlayers.Count > 0)
            {
                Player groupPlayer = unassignedPlayers[Random.Range(0, unassignedPlayers.Count)];
                unassignedPlayers.Remove(groupPlayer);
                groups[curGroup].Add(groupPlayer);
                curGroup++;
                if (curGroup >= numGroups) curGroup = 0;
            }
            // Group stage
            for (int i = 0; i < 4; i++)
            {
                Match groupMatch = new Match("Group " + (i + 1), this, Quarter, Day, numPlayers: groups[i].Count, PointDistributions[groups[i].Count]);
                foreach (Player p in groups[i]) groupMatch.AddPlayerToMatch(p, seed: 0);
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
    }

    public override void DisplayTournament(UI_Base baseUI, GameObject Container)
    {
        int[] playersPerPhase;
        int[] matchesPerPhase;

        if(Players.Count == 0)
        {
            playersPerPhase = new int[] { };
            matchesPerPhase = new int[] { };
        }
        else if(Players.Count < 6)
        {
            playersPerPhase = new int[] { 6 };
            matchesPerPhase = new int[] { 1 };
        }
        else if(Players.Count < 12)
        {
            playersPerPhase = new int[] { 6, 4 };
            matchesPerPhase = new int[] { 2, 1 };
        }
        else if(Players.Count < 24)
        {
            playersPerPhase = new int[] { 6, 4, 4 };
            matchesPerPhase = new int[] { 4, 2, 1 };
        }
        else
        {
            int maxGroupSize = Players.Count / 4;
            playersPerPhase = new int[] { maxGroupSize, 6, 4, 4 };
            matchesPerPhase = new int[] { 4, 4, 2, 1 };
        }

        DisplayTournamentAsLayers(baseUI, Container, playersPerPhase, matchesPerPhase);
    }

    protected override void OnTournamentDone()
    {
        if (Players.Count == 0) return;
        if (Players.Count < 6)
        {
            for (int i = 0; i < Matches[0].PlayerRanking.Count; i++) League.Standings[Matches[0].PlayerRanking[i]] += Players.Count - i;
        }
        else if (Players.Count < 12)
        {
            // Final
            League.Standings[Matches[2].PlayerRanking[3]] += 1;
            League.Standings[Matches[2].PlayerRanking[2]] += 2;
            League.Standings[Matches[2].PlayerRanking[1]] += 3;
            League.Standings[Matches[2].PlayerRanking[0]] += 4;
        }
        else if (Players.Count < 24)
        {
            // Semis
            for (int i = 4; i < 6; i++)
            {
                League.Standings[Matches[i].PlayerRanking[2]] += 1;
                League.Standings[Matches[i].PlayerRanking[3]] += 1;
            }
            // Final
            League.Standings[Matches[6].PlayerRanking[3]] += 2;
            League.Standings[Matches[6].PlayerRanking[2]] += 3;
            League.Standings[Matches[6].PlayerRanking[1]] += 4;
            League.Standings[Matches[6].PlayerRanking[0]] += 5;
        }
        else
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
    }

    public override string GetMatchDayTitle(int index)
    {
        int stepIndex = Database.Tournaments.Values.Where(x => x.League == League).OrderBy(x => x.Matches[0].Quarter).ThenBy(x => x.Matches[0].Day).ToList().IndexOf(this) + 1;
        return "Step " + stepIndex;
    }
}
