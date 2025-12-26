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

    protected const int GRAND_FINAL_STARTING_LIVES = 3;
    protected const int GRAND_FINAL_LIVE_GAINERS = 1;
    protected const int GRAND_FINAL_LIVE_LOSERS = 2;


    public const int MAX_PARTICIPANTS = INITIAL_ROUND_MATCHES * INITIAL_ROUND_MAX_PPM;

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
    private const int INITIAL_ROUND_MIN_PPM = 22;
    private const int INITIAL_ROUND_MAX_PPM = 64;
    private const int R1_UPPER_PPM = 20; // 4 matches. per match: 8 to R2 upper, 12 to R2 lower 
    private const int R1_LOWER_MIN_PPM = 2; // 4 matches. per match: 12 to R2 lower, rest out
    private const int R1_LOWER_MAX_PPM = 44; // 4 matches. per match: 12 to R2 lower, rest out
    private const int R2_UPPER_PPM = 16; // 2 matches. per match: 6 to R3 upper, 10 to R3 lower 
    private const int R2_LOWER_MIN_PPM = 12; // 4 matches. per match: 11 to R4 lower, 13 out
    private const int R2_LOWER_MAX_PPM = 24; // 4 matches. per match: 11 to R4 lower, 13 out
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

    // Losers per stage
    private const int R1_UPPER_LOSERS = R1_UPPER_PPM - R1_UPPER_ADV;
    private const int R2_UPPER_LOSERS = R2_UPPER_PPM - R2_UPPER_ADV;
    private const int R3_UPPER_LOSERS = R3_UPPER_PPM - R3_UPPER_ADV;

    // Match lists for individual phases (only used as cache during tournament creation)
    private List<Match> InitialRoundMatches = new List<Match>();
    private List<Match> R1UpperMatches = new List<Match>();
    private List<Match> R1LowerMatches = new List<Match>();
    private List<Match> R2UpperMatches = new List<Match>();
    private List<Match> R2LowerMatches = new List<Match>();
    private List<Match> R3UpperMatches = new List<Match>();
    private List<Match> R3LowerMatches = new List<Match>();
    private List<Match> R4LowerMatches = new List<Match>();
    private List<Match> SemiFinalMatches = new List<Match>();
    private Match GrandFinalMatch;

    public Format_BigCup(TournamentData data) : base(data) { }
    public Format_BigCup(DisciplineDef disciplineDef, int season, int quarter, int day) : base(disciplineDef, TournamentType.BIGCup, season)
    {
        Name = "BIG Cup";
        Quarter = quarter;
        Day = day;

        InitMatches();
        InitAdvancements();
        InitModifiers();
    }

    public override void OnTournamentStart()
    {
        // Seed tournament
        List<Player> orderedParticipants = Database.GetPlayerEloRanking(Discipline.Def);

        if (orderedParticipants.Count > MAX_PARTICIPANTS) orderedParticipants = orderedParticipants.Take(MAX_PARTICIPANTS).ToList();

        List<Match> initalRoundMatches = Matches.Take(INITIAL_ROUND_MATCHES).ToList();
        Seeder.SnakeSeedSoloTournament(orderedParticipants, initalRoundMatches);

        Players = new List<Player>(orderedParticipants);
    }

    /// <summary>
    /// Creates all matches. No players are assigned at this stage and no connection between matches are created.
    /// </summary>
    private void InitMatches()
    {
        Matches = new List<Match>();
        Groups = new List<TournamentGroup>();

        // Initial Round
        for (int i = 0; i < INITIAL_ROUND_MATCHES; i++)
        {
            Match initialMatch = new SoloMatch("Starting Phase - Match " + (i + 1), this, Quarter, Day, MatchFormatDefOf.SingleGame, maxPlayers: INITIAL_ROUND_MAX_PPM, FirstStagePointDistribution, minPlayers: INITIAL_ROUND_MIN_PPM);

            InitialRoundMatches.Add(initialMatch);
            Matches.Add(initialMatch);
        }

        // R1 UPPER
        for (int i = 0; i < R1_UPPER_MATCHES; i++)
        {
            Match match = new SoloMatch("Winner Bracket - Round 1 - Match " + (i + 1), this, Quarter, Day, MatchFormatDefOf.SingleGame, maxPlayers: R1_UPPER_PPM, M20PointDistribution);

            R1UpperMatches.Add(match);
            Matches.Add(match);
        }

        // R1 LOWER
        for (int i = 0; i < R1_LOWER_MATCHES; i++)
        {
            Match match = new SoloMatch("Lower Bracket - Round 1 - Match " + (i + 1), this, Quarter, Day, MatchFormatDefOf.SingleGame, maxPlayers: R1_LOWER_MAX_PPM, FirstStagePointDistribution, minPlayers: R1_LOWER_MIN_PPM);

            R1LowerMatches.Add(match);
            Matches.Add(match);
        }

        // R2 UPPER
        for (int i = 0; i < R2_UPPER_MATCHES; i++)
        {
            Match match = new SoloMatch("Winner Bracket - Round 2 - Match " + (i + 1), this, Quarter, Day, MatchFormatDefOf.SingleGame, maxPlayers: R2_UPPER_PPM, M16PointDistribution);

            R2UpperMatches.Add(match);
            Matches.Add(match);
        }

        // R2 LOWER
        for (int i = 0; i < R2_LOWER_MATCHES; i++)
        {
            Match match = new SoloMatch("Lower Bracket - Round 2 - Match " + (i + 1), this, Quarter, Day, MatchFormatDefOf.SingleGame, maxPlayers: R2_LOWER_MAX_PPM, M24PointDistribution, minPlayers: R2_LOWER_MIN_PPM);

            R2LowerMatches.Add(match);
            Matches.Add(match);
        }

        // R3 UPPER
        for (int i = 0; i < R3_UPPER_MATCHES; i++)
        {
            Match match = new SoloMatch("Winner Bracket Finals", this, Quarter, Day, MatchFormatDefOf.SingleGame, maxPlayers: R3_UPPER_PPM, M12PointDistribution);

            R3UpperMatches.Add(match);
            Matches.Add(match);
        }

        // R3 LOWER
        for (int i = 0; i < R3_LOWER_MATCHES; i++)
        {
            Match match = new SoloMatch("Lower Bracket - Round 3 - Match " + (i + 1), this, Quarter, Day, MatchFormatDefOf.SingleGame, maxPlayers: R3_LOWER_PPM, M32PointDistribution);

            R3LowerMatches.Add(match);
            Matches.Add(match);
        }

        // R4 LOWER
        for (int i = 0; i < R4_LOWER_MATCHES; i++)
        {
            Match match = new SoloMatch("Lower Bracket Finals - Match " + (i + 1), this, Quarter, Day, MatchFormatDefOf.SingleGame, maxPlayers: R4_LOWER_PPM, M8PointDistribution);

            R4LowerMatches.Add(match);
            Matches.Add(match);
        }

        // SEMI
        for (int i = 0; i < SEMI_FINAL_MATCHES; i++)
        {
            Match match = new SoloMatch("Semifinal " + (i + 1), this, Quarter, Day, MatchFormatDefOf.SingleGame, maxPlayers: SEMI_FINAL_PPM, M8PointDistribution);

            SemiFinalMatches.Add(match);
            Matches.Add(match);
        }

        // FINAL
        GrandFinalMatch = new SoloMatch("Grand Final", this, Quarter, Day, MatchFormatDefOf.SingleGame, maxPlayers: GRAND_FINAL_PPM, isKnockout: true, knockoutStartingLives: GRAND_FINAL_STARTING_LIVES, koLiveGainers: GRAND_FINAL_LIVE_GAINERS, koLiveLosers: GRAND_FINAL_LIVE_LOSERS);
        Matches.Add(GrandFinalMatch);
    }

    /// <summary>
    /// Creates all connections between matches, meaning which ranks in the matches lead to which other matches in the tournament.
    /// </summary>
    private void InitAdvancements()
    {
        // Initial -> R1 Upper
        Seeder.CreateSnakeSeededAdvancements(InitialRoundMatches, R1UpperMatches, INITIAL_ROUND_ADV);
        // Initial -> R1 Lower
        Seeder.CreateSnakeSeededAdvancements(InitialRoundMatches, R1LowerMatches, numAdvancements: -1, advancementOffset: INITIAL_ROUND_ADV);

        // R1 Upper -> R2 Upper
        Seeder.CreateSnakeSeededAdvancements(R1UpperMatches, R2UpperMatches, numAdvancements: R1_UPPER_ADV);
        // R1 Upper -> R2 Lower
        Seeder.CreateSnakeSeededAdvancements(R1UpperMatches, R2LowerMatches, numAdvancements: -1, advancementOffset: R1_UPPER_ADV);
        // R1 Lower -> R2 Lower
        int r12targetSeedOffset = (R1_UPPER_LOSERS * R1_UPPER_MATCHES) / R2_LOWER_MATCHES;
        Seeder.CreateSnakeSeededAdvancements(R1LowerMatches, R2LowerMatches, numAdvancements: R1_LOWER_ADV, targetSeedOffset: r12targetSeedOffset);

        // R2 Upper -> R3 Upper
        Seeder.CreateSnakeSeededAdvancements(R2UpperMatches, R3UpperMatches, numAdvancements: R2_UPPER_ADV);
        // R2 Upper -> R3 Lower
        Seeder.CreateSnakeSeededAdvancements(R2UpperMatches, R3LowerMatches, numAdvancements: -1, advancementOffset: R2_UPPER_ADV);
        // R2 Lower -> R3 Lower
        int r23targetSeedOffset = (R2_UPPER_LOSERS * R2_UPPER_MATCHES) / R3_LOWER_MATCHES;
        Seeder.CreateSnakeSeededAdvancements(R2LowerMatches, R3LowerMatches, numAdvancements: R2_LOWER_ADV, targetSeedOffset: r23targetSeedOffset);

        // R3 Upper -> SemiFinals
        Seeder.CreateSnakeSeededAdvancements(R3UpperMatches, SemiFinalMatches, numAdvancements: R3_UPPER_ADV);
        // R3 Upper -> R4 Lower
        Seeder.CreateSnakeSeededAdvancements(R3UpperMatches, R4LowerMatches, numAdvancements: -1, advancementOffset: R3_UPPER_ADV);
        // R3 Lower -> R4 Lower
        int r34targetSeedOffset = (R3_UPPER_LOSERS * R3_UPPER_MATCHES) / R4_LOWER_MATCHES;
        Seeder.CreateSnakeSeededAdvancements(R3LowerMatches, R4LowerMatches, numAdvancements: R3_LOWER_ADV, targetSeedOffset: r34targetSeedOffset);

        // R4 Lower -> SemiFinals
        int semiTargetSeedOffset = (R3_UPPER_ADV * R3_UPPER_MATCHES) / SEMI_FINAL_MATCHES;
        Seeder.CreateSnakeSeededAdvancements(R4LowerMatches, SemiFinalMatches, numAdvancements: R4_LOWER_ADV, targetSeedOffset: semiTargetSeedOffset);

        // SemiFinals -> GrandFinal
        Seeder.CreateSnakeSeededAdvancements(SemiFinalMatches, new List<Match>() { GrandFinalMatch }, numAdvancements: SEMI_FINAL_ADV);
    }

    private void InitModifiers()
    {
        Modifiers = new List<GameModifierDef>() { GameModifierDefOf.BIGCup };
    }
    public override void DisplayTournament(UI_Base baseUI, GameObject container)
    {
        List<List<List<Match>>> orderedMatches = new List<List<List<Match>>>();
        int globalIndex = 0;
        
        List<List<Match>> round1 = new List<List<Match>>();
        AddBracketGroupMatchesToRound(round1, INITIAL_ROUND_MATCHES, ref globalIndex);
        orderedMatches.Add(round1);

        List<List<Match>> round2 = new List<List<Match>>();
        AddBracketGroupMatchesToRound(round2, R1_UPPER_MATCHES, ref globalIndex);
        AddBracketGroupMatchesToRound(round2, R1_LOWER_MATCHES, ref globalIndex);
        orderedMatches.Add(round2);

        List<List<Match>> round3 = new List<List<Match>>();
        AddBracketGroupMatchesToRound(round3, R2_UPPER_MATCHES, ref globalIndex);
        AddBracketGroupMatchesToRound(round3, R2_LOWER_MATCHES, ref globalIndex);
        orderedMatches.Add(round3);

        List<List<Match>> round4 = new List<List<Match>>();
        AddBracketGroupMatchesToRound(round4, R3_UPPER_MATCHES, ref globalIndex);
        AddBracketGroupMatchesToRound(round4, R3_LOWER_MATCHES, ref globalIndex);
        orderedMatches.Add(round4);

        List<List<Match>> round5 = new List<List<Match>>();
        round5.Add(new List<Match>()); // Dummy list so the other list is recognized as lower
        AddBracketGroupMatchesToRound(round5, R4_LOWER_MATCHES, ref globalIndex);
        orderedMatches.Add(round5);

        List<List<Match>> round6 = new List<List<Match>>();
        AddBracketGroupMatchesToRound(round6, SEMI_FINAL_MATCHES, ref globalIndex);
        orderedMatches.Add(round6);

        List<List<Match>> round7 = new List<List<Match>>();
        AddBracketGroupMatchesToRound(round7, 1, ref globalIndex);
        orderedMatches.Add(round7);

        DisplayLayeredDoubleElimBracket(baseUI, container, orderedMatches);
    }

    private void AddBracketGroupMatchesToRound(List<List<Match>> round, int numMatches, ref int globalIndex)
    {
        List<Match> matches = new List<Match>();
        for (int i = 0; i < numMatches; i++) matches.Add(Matches[globalIndex++]);
        round.Add(matches);
    }

    public override string GetMatchDayTitle(int index) => Name;

    public override Dictionary<int, List<Player>> PlayerRanking => Matches.Last().GetPlayerRankingWithRanks();

    public override Dictionary<int, List<Team>> TeamRanking => PlayerRanking.ToDictionary(x => x.Key, x => x.Value.Select(x => Database.GetNationalTeam(x.Country)).ToList());
}
