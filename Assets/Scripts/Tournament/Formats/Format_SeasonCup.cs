using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Format_SeasonCup : Tournament
{
    private static int RO64_QUARTER = 1;
    private static int RO64_DAY = 15;

    private static int RO32_QUARTER = 2;
    private static int RO32_DAY = 1;

    private static int RO16_QUARTER = 2;
    private static int RO16_DAY = 12;

    private static int QUARTERS_QUARTER = 3;
    private static int QUARTERS_DAY = 5;

    private static int SEMIS_QUARTER = 3;
    private static int SEMIS_DAY = 12;

    private static int FINALS_QUARTER = 4;
    private static int FINALS_DAY = 5;

    protected List<int> PointDistribution = new List<int>() { 1, 0 };

    public Format_SeasonCup(TournamentData data) : base(data) { }
    public Format_SeasonCup(int season) : base(TournamentType.SeasonCup, season)
    {
        Name = "Season Cup";
        Players = Database.WorldRanking.Take(64).ToList();

        Initialize();
    }

    public override void Initialize()
    {
        if (Players.Count != 64) throw new System.Exception("Season cup must have exactly 64 players and not " + Players.Count);
        if (Teams.Count != 0) throw new System.Exception("Player tournaments cannot have team participants, only players.");

        Groups = new List<TournamentGroup>();
        Matches = new List<Match>();

        // RO64 (random seeding to fill initial slots)
        List<Player> unassignedPlayers = new List<Player>();
        unassignedPlayers.AddRange(Players);
        for (int i = 0; i < 32; i++)
        {
            Match ro64Match = new Match("RO64 - Match " + (i + 1), this, RO64_QUARTER, RO64_DAY, numPlayers: 2, PointDistribution);

            // Add initial players
            for (int j = 0; j < 2; j++)
            {
                Player p = unassignedPlayers[Random.Range(0, unassignedPlayers.Count)];
                unassignedPlayers.Remove(p);
                ro64Match.AddPlayerToMatch(p, seed: 0);
            }

            // Advancement matches
            ro64Match.SetTargetMatches(new List<int>() { 32 + (i / 2) });

            Matches.Add(ro64Match);
        }

        // RO32
        for (int i = 0; i < 16; i++)
        {
            Match ro32Match = new Match("RO32 - Match " + (i + 1), this, RO32_QUARTER, RO32_DAY, numPlayers: 2, PointDistribution);

            // Advancement matches
            ro32Match.SetTargetMatches(new List<int>() { 48 + (i / 2) });

            Matches.Add(ro32Match);
        }

        // RO16
        for (int i = 0; i < 8; i++)
        {
            Match ro16Match = new Match("RO16 - Match " + (i + 1), this, RO16_QUARTER, RO16_DAY, numPlayers: 2, PointDistribution);

            // Advancement matches
            ro16Match.SetTargetMatches(new List<int>() { 56 + (i / 2) });

            Matches.Add(ro16Match);
        }

        // Quarterfinals
        for (int i = 0; i < 4; i++)
        {
            Match quarterfinal = new Match("Quarterfinal " + (i + 1), this, QUARTERS_QUARTER, QUARTERS_DAY, numPlayers: 2, PointDistribution);

            // Advancement matches
            quarterfinal.SetTargetMatches(new List<int>() { 60 + (i / 2) });

            Matches.Add(quarterfinal);
        }

        // Semifinals
        for (int i = 0; i < 2; i++)
        {
            Match semifinal = new Match("Semifinal " + (i + 1), this, SEMIS_QUARTER, SEMIS_DAY, numPlayers: 2, PointDistribution);

            // Advancement matches
            semifinal.SetTargetMatches(new List<int>() { 63, 62 });

            Matches.Add(semifinal);
        }

        // Match for place 3
        Matches.Add(new Match("Match for place 3", this, FINALS_QUARTER, FINALS_DAY, numPlayers: 2, PointDistribution));

        // Final
        Matches.Add(new Match("Final", this, FINALS_QUARTER, FINALS_DAY, numPlayers: 2, PointDistribution));
    }

    public override void DisplayTournament(UI_Base baseUI, GameObject Container)
    {
        int[] playersPerPhase = new int[] { 2, 2, 2, 2, 2, 2 };
        int[] matchesPerPhase = new int[] { 32, 16, 8, 4, 2, 2 };
        DisplayAsDynamicTableau(baseUI, Container, playersPerPhase, matchesPerPhase);
    }

    public override string GetMatchDayTitle(int index)
    {
        return index switch
        {
            0 => "Ro64",
            1 => "Ro32",
            2 => "Ro16",
            3 => "Quarters",
            4 => "Semifinals",
            5 => "Finals",
            _ => ""
        };
    }
}
