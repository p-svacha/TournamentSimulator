using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Format_SeasonCup : Tournament
{
    public static int NUM_PARTICIPANTS = 64;

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
    public Format_SeasonCup(DisciplineDef disciplineDef, int season) : base(disciplineDef, TournamentType.SeasonCup, season)
    {
        Name = "Season Cup";

        InitMatches();
        InitMatchAdvancements();
    }

    public override void OnTournamentStart()
    {
        // Seed tournament
        Players = Database.GetPlayerEloRanking(Discipline.Def).Take(NUM_PARTICIPANTS).ToList();

        List<Match> initalRoundMatches = Matches.Take(NUM_PARTICIPANTS / 2).ToList();
        Seeder.SeedSingleElimTournament(Players, initalRoundMatches);
    }

    public void InitMatches()
    {
        if (Teams.Count != 0) throw new System.Exception("Player tournaments cannot have team participants, only players.");

        Groups = new List<TournamentGroup>();
        Matches = new List<Match>();

        // RO64
        for (int i = 0; i < 32; i++)
        {
            Match ro64Match = new SoloMatch("RO64 - Match " + (i + 1), this, RO64_QUARTER, RO64_DAY, MatchFormatDefOf.SingleGame, maxPlayers: 2, PointDistribution);

            Matches.Add(ro64Match);
        }

        // RO32
        for (int i = 0; i < 16; i++)
        {
            Match ro32Match = new SoloMatch("RO32 - Match " + (i + 1), this, RO32_QUARTER, RO32_DAY, MatchFormatDefOf.SingleGame, maxPlayers: 2, PointDistribution);

            Matches.Add(ro32Match);
        }

        // RO16
        for (int i = 0; i < 8; i++)
        {
            Match ro16Match = new SoloMatch("RO16 - Match " + (i + 1), this, RO16_QUARTER, RO16_DAY, MatchFormatDefOf.SingleGame, maxPlayers: 2, PointDistribution);

            Matches.Add(ro16Match);
        }

        // Quarterfinals
        for (int i = 0; i < 4; i++)
        {
            Match quarterfinal = new SoloMatch("Quarterfinal " + (i + 1), this, QUARTERS_QUARTER, QUARTERS_DAY, MatchFormatDefOf.SingleGame, maxPlayers: 2, PointDistribution);

            Matches.Add(quarterfinal);
        }

        // Semifinals
        for (int i = 0; i < 2; i++)
        {
            Match semifinal = new SoloMatch("Semifinal " + (i + 1), this, SEMIS_QUARTER, SEMIS_DAY, MatchFormatDefOf.SingleGame, maxPlayers: 2, PointDistribution);

            Matches.Add(semifinal);
        }

        // Match for place 3
        Matches.Add(new SoloMatch("Match for place 3", this, FINALS_QUARTER, FINALS_DAY, MatchFormatDefOf.SingleGame, maxPlayers: 2, PointDistribution));

        // Final
        Matches.Add(new SoloMatch("Final", this, FINALS_QUARTER, FINALS_DAY, MatchFormatDefOf.SingleGame, maxPlayers: 2, PointDistribution));
    }

    private void InitMatchAdvancements()
    {
        // RO64
        int globalIndex = 0;
        for (int i = 0; i < 32; i++) Matches[globalIndex++].SetTargetMatches(new List<int>() { 32 + (i / 2) }, new List<int>() { i % 2 });

        // RO32
        for (int i = 0; i < 16; i++) Matches[globalIndex++].SetTargetMatches(new List<int>() { 48 + (i / 2) }, new List<int>() { i % 2 });

        // RO16
        for (int i = 0; i < 8; i++) Matches[globalIndex++].SetTargetMatches(new List<int>() { 56 + (i / 2) }, new List<int>() { i % 2 });

        // Quarterfinals
        for (int i = 0; i < 4; i++) Matches[globalIndex++].SetTargetMatches(new List<int>() { 60 + (i / 2) }, new List<int>() { i % 2 });

        // Semifinals
        for (int i = 0; i < 2; i++) Matches[globalIndex++].SetTargetMatches(new List<int>() { 63, 62 }, new List<int>() { i % 2, i % 2 });
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

    public override Dictionary<int, List<Player>> PlayerRanking
    {
        get
        {
            Dictionary<int, List<Player>> ranking = new Dictionary<int, List<Player>>();
            ranking.Add(0, new List<Player>() { Matches[Matches.Count - 1].GetPlayerRanking()[0].Player });
            ranking.Add(1, new List<Player>() { Matches[Matches.Count - 1].GetPlayerRanking()[1].Player });
            ranking.Add(2, new List<Player>() { Matches[Matches.Count - 2].GetPlayerRanking()[0].Player });
            ranking.Add(3, new List<Player>() { Matches[Matches.Count - 2].GetPlayerRanking()[1].Player });
            return ranking;
        }
    }

    public override Dictionary<int, List<Team>> TeamRanking => PlayerRanking.ToDictionary(x => x.Key, x => x.Value.Select(x => Database.GetNationalTeam(x.Country)).ToList());
}
