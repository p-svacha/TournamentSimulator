using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Seasonal, single-day, obstacle-themed, double-elimination tournament where everyone participates with an 8 player knockout final.
/// </summary>
public class Format_BigCup : Tournament
{
    public int Quarter { get; private set; }
    public int Day { get; private set; }

    protected List<int> FirstStagePointDistribution = new List<int>()
    {
        250,
        220,
        200,
        190,
        180, // 5th
        172,
        164,
        156,
        149, 
        142, // 10th
        135,
        129,
        123,
        117,
        111, // 15th
        106,
        101,
        96,
        91,
        86, // 20th
        82,
        78,
        74,
        70,
        66, // 25th
        63,
        60,
        57,
        54,
        51, // 30th
        48,
        45,
        42,
        40,
        38, // 35th
        36,
        34,
        32,
        30,
        28, // 40th
        26,
        24,
        22,
        20,
        19, // 45th
        18,
        17,
        16,
        15,
        14, // 50th
        13,
        12,
        11,
        10,
        9, // 55th
        8,
        7,
        6,
        5,
        4, // 60th
        3,
        2,
        1,
        1,
    };

    protected List<int> M8PointDistribution = new List<int>() { 10, 8, 6, 5, 4, 3, 2, 1 };
    protected List<int> M12PointDistribution = new List<int>() { 15, 12, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
    protected List<int> M16PointDistribution = new List<int>()
    {
        20, 17, 15, 13, 12, 11, 10, 9,
         8,  7,  6,  5,  4,  3, 2, 1
    };
    protected List<int> M20PointDistribution = new List<int>()
    {
        25, 22, 20, 18, 16, 15, 14, 13, 12, 11,
        10,  9,  8,  7,  6,  5,  4,  3, 2, 1
    };
    protected List<int> M24PointDistribution = new List<int>()
    {
        30, 26, 23, 21, 20, 19, 18, 17, 16, 15, 14, 13,
        12, 11, 10,  9,  8,  7,  6,  5,  4,  3, 2, 1
    };
    protected List<int> M32PointDistribution = new List<int>()
    {
        40, 36, 35, 32, 30, 28, 26, 24,
        23, 22, 21, 20, 19, 18, 17, 16,
        15, 14, 13, 12, 11, 10,  9,  8,
         7,  6,  5,  4,  3,  2, 1
    };

    protected List<int> GrandFinalPointDistribution = new List<int>() { 0, 0, 0, 0, 0, 0, 0, -1 };

    // Number of matches
    private const int INITIAL_ROUND_MATCHES = 4;
    private const int R1_UPPER_MATCHES = 4;
    private const int R1_LOWER_MATCHES = 4;
    private const int R2_UPPER_MATCHES = 2;
    private const int R2_LOWER_MATCHES = 4;
    private const int R3_UPPER_MATCHES = 1;
    private const int R3_LOWER_MATCHES = 2;
    private const int R4_LOWER_MATCHES = 4;
    private const int SEMI_FINAL_MATCHES = 2;
    private const int GRAND_FINAL_MATCHES = 1;

    // Players per Match (PPM)
    private const int INITIAL_ROUND_PPM = -1; // 4 matches. per match: 20 to R1 upper, rest to R2 lower 
    private const int R1_UPPER_PPM = 20; // 4 matches. per match: 8 to R2 upper, 12 to R2 lower 
    private const int R1_LOWER_PPM = -1; // 4 matches. per match: 12 to R2 lower, rest out
    private const int R2_UPPER_PPM = 16; // 2 matches. per match: 6 to R3 upper, 10 to R3 lower 
    private const int R2_LOWER_PPM = 24; // 4 matches. per match: 11 to R4 lower, 13 out
    private const int R3_UPPER_PPM = 12; // 1 match. 4 to SemiFinal, 8 to R4 lower
    private const int R3_LOWER_PPM = 32; // 2 matches. per match: 12 to R4 lower, 24 out
    private const int R4_LOWER_PPM = 8; // 4 matches. per match: 3 to SemiFinal, 5 out
    private const int SEMI_FINAL_PPM = 8; // 2 matches. per match: 4 to GrandFinal, 4 out
    private const int GRAND_FINAL_PPM = 8;

    // Advancements to next stage
    private const int INITIAL_ROUND_ADV = 20;
    private const int R1_UPPER_ADV = 8;
    private const int R1_LOWER_ADV = 12;
    private const int R2_UPPER_ADV = 6;
    private const int R2_LOWER_ADV = 11;
    private const int R3_UPPER_ADV = 4;
    private const int R3_LOWER_ADV = 12;
    private const int R4_LOWER_ADV = 3;
    private const int SEMI_FINAL_ADV = 4;

    // Match lists for individual phases (only used as cache during tournament creation)
    private List<Match> InitialRoundMatches = new List<Match>();

    public Format_BigCup(TournamentData data) : base(data) { }
    public Format_BigCup(DisciplineDef disciplineDef, int season, int quarter, int day) : base(disciplineDef, TournamentType.SeasonCup, season)
    {
        Name = "BIG Cup";
        Quarter = quarter;
        Day = day;

        InitMatches();
        InitAdvancements();
    }

    /// <summary>
    /// Should be called the day the tournament starts. Fills all slots in the initial round with players, seeded according to their current global rank.
    /// </summary>
    public void SeedTournament()
    {
        List<Player> orderedParticipants = Database.GetPlayerEloRanking(Discipline.Def);

        // todo: fill matches
    }

    /// <summary>
    /// Creates all matches. No players are assigned at this stage and no connection between matches are created.
    /// </summary>
    private void InitMatches()
    {
        // Initial Round
        for (int i = 0; i < INITIAL_ROUND_MATCHES; i++)
        {
            Match initialMatch = new SoloMatch("Starting Phase - Match " + (i + 1), this, Quarter, Day, MatchFormatDefOf.SingleGame, numPlayers: INITIAL_ROUND_PPM, FirstStagePointDistribution);

            InitialRoundMatches.Add(initialMatch);
            Matches.Add(initialMatch);
        }

        // R1 UPPER
        // todo

        // R1 LOWER
        // todo

        // R2 UPPER
        // todo

        // R2 LOWER
        // todo

        // R3 UPPER
        // todo

        // R3 LOWER
        // todo

        // R4 LOWER
        // todo

        // SEMI
        // todo

        // FINAL
        // todo
    }

    /// <summary>
    /// Creates all connections between matches, meaning which ranks in the matches lead to which other matches in the tournament.
    /// </summary>
    private void InitAdvancements()
    {

    }

    public override void DisplayTournament(UI_Base baseUI, GameObject Container)
    {
        // todo, display as layers from left to right, with winner bracket in upper half and loser bracket in lower. first stage, semis and grand final in center.

        /* this will get replaced
        int[] playersPerPhase = new int[] { 2, 2, 2, 2, 2, 2 };
        int[] matchesPerPhase = new int[] { 32, 16, 8, 4, 2, 2 };
        DisplayAsDynamicTableau(baseUI, Container, playersPerPhase, matchesPerPhase);
        */
    }

    public override string GetMatchDayTitle(int index) => Name;

    public override Dictionary<int, List<Player>> PlayerRanking => Matches.Last().GetPlayerRankingWithRanks();

    public override Dictionary<int, List<Team>> TeamRanking => PlayerRanking.ToDictionary(x => x.Key, x => x.Value.Select(x => Database.GetNationalTeam(x.Country)).ToList());
}
