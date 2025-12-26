using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Format_WorldCup : Tournament
{
    private static int POINTS_FOR_WIN = 3;
    private static int POINTS_FOR_DRAW = 1;
    private static int POINTS_FOR_LOSS = 0;

    private static int START_QUARTER = 4;
    private static int START_DAY = 6;

    private List<int> ValidTeamAmounts = new List<int>() { 16 };
    private List<int> TeamPointDistribution = new List<int>() { 1, 0 };

    private const int NUM_GROUPS = 4;
    private const int NUM_TEAMS_PER_GROUP = 4;
    private const int NUM_PARTICIPATING_TEAMS = NUM_GROUPS * NUM_TEAMS_PER_GROUP;

    public Format_WorldCup(TournamentData data) : base(data) { }
    public Format_WorldCup(DisciplineDef disciplineDef, int season) : base(disciplineDef, TournamentType.WorldCup, season)
    {
        // Get maximum amount of players per team so there are at least 16 eligible teams
        int numPlayersPerTeam = 1;
        bool hasEnoughTeams = true;
        while (hasEnoughTeams)
        {
            numPlayersPerTeam++;
            List<Team> eligibleTeams = Database.GetNationalTeams(minPlayers: numPlayersPerTeam);
            hasEnoughTeams = eligibleTeams.Count >= NUM_PARTICIPATING_TEAMS;
        }
        NumPlayersPerTeam = numPlayersPerTeam - 1;

        Name = "S" + season + " World Cup (" + NumPlayersPerTeam + "v" + NumPlayersPerTeam + ")";

        InitBracket();
    }

    public override void OnTournamentStart()
    {
        // Get list of national teams that have enough players
        List<Team> eligibleTeams = Database.GetNationalTeams(minPlayers: NumPlayersPerTeam);

        // Select participants by average player elo
        Teams = eligibleTeams.OrderByDescending(x => x.GetAveragePlayerElo(Discipline.Def, NumPlayersPerTeam)).Take(NUM_PARTICIPATING_TEAMS).ToList();

        // Order particiting teams by team elo for seeding
        Teams = Teams.OrderByDescending(t => t.Elo[Discipline.Def]).ToList();

        // Seed into groups
        Seeder.SnakeSeedTeamsIntoGroups(Teams, Groups);
    }

    public void InitBracket()
    {
        // Validate
        if (!ValidTeamAmounts.Contains(Teams.Count)) throw new System.Exception(Teams.Count + " is not a valid amount of teams for a world cup.");
        if (Players.Count != 0) throw new System.Exception("Team tournaments cannot have player participants, only teams");

        // Create groups and matches based on team amount
        Groups = new List<TournamentGroup>();
        Matches = new List<Match>();

        if (Teams.Count == 16) CreateWorldCup16();
        else throw new System.Exception(Teams.Count + " not handled in match creation");
    }

    private void CreateWorldCup16()
    {
        int groupsStartDayAbsolute = Database.ToAbsoluteDay(Season, START_QUARTER, START_DAY);

        // Group phase (match index 0-23)
        for(int i = 0; i < NUM_GROUPS; i++)
        {
            // Calculate target match indices
            List<int> targetMatchIndices = new List<int>() { 24 + (i % 4), 24 + ((i + 1) % 4) };

            // Create group
            TournamentGroup group = new TournamentGroup(this, "Group " + HelperFunctions.GetIndexLetter(i), groupSize: NUM_TEAMS_PER_GROUP, POINTS_FOR_WIN, POINTS_FOR_DRAW, POINTS_FOR_LOSS, groupsStartDayAbsolute, targetMatchIndices);
            Groups.Add(group);
        }

        // Quarters (match index 24-27)
        int quartersStartDayAbsolute = groupsStartDayAbsolute + (NUM_TEAMS_PER_GROUP - 1);
        int quartersQuarter = Database.ToRelativeQuarter(quartersStartDayAbsolute);
        int quartersDay = Database.ToRelativeDay(quartersStartDayAbsolute);
        for (int i = 0; i < 4; i++)
        {
            TeamMatch quarterfinal = new TeamMatch("Quarterfinal " + (i + 1), this, quartersQuarter, quartersDay, MatchFormatDefOf.SingleGame, numTeams: 2, NumPlayersPerTeam, TeamPointDistribution, GetBasicPointDistribution(NumPlayersPerTeam * 2));
            quarterfinal.SetTargetMatches(new List<int>() { 28 + (i / 2) }, new List<int>() { i % 2 });
            Matches.Add(quarterfinal);
        }

        // Semis (match index 28-29)
        int semisStartDayAbsolute = quartersStartDayAbsolute + 1;
        int semisQuarter = Database.ToRelativeQuarter(semisStartDayAbsolute);
        int semisDay = Database.ToRelativeDay(semisStartDayAbsolute);
        for (int i = 0; i < 2; i++)
        {
            TeamMatch semifinal = new TeamMatch("Semifinal " + (i + 1), this, semisQuarter, semisDay, MatchFormatDefOf.SingleGame, numTeams: 2, NumPlayersPerTeam, TeamPointDistribution, GetBasicPointDistribution(NumPlayersPerTeam * 2));
            semifinal.SetTargetMatches(new List<int>() { 31, 30 }, new List<int>() { i % 2, i % 2 });
            Matches.Add(semifinal);
        }

        // Final day
        int finalsDayAbsolute = semisStartDayAbsolute + 1;
        int finalsQuarter = Database.ToRelativeQuarter(finalsDayAbsolute);
        int finalsDay = Database.ToRelativeDay(finalsDayAbsolute);

        // Match for place 3
        Matches.Add(new TeamMatch("Match for place 3", this, finalsQuarter, finalsDay, MatchFormatDefOf.SingleGame, numTeams: 2, NumPlayersPerTeam, TeamPointDistribution, GetBasicPointDistribution(NumPlayersPerTeam * 2)));

        // Final
        Matches.Add(new TeamMatch("Final", this, finalsQuarter, finalsDay, MatchFormatDefOf.SingleGame, numTeams: 2, NumPlayersPerTeam, TeamPointDistribution, GetBasicPointDistribution(NumPlayersPerTeam * 2)));
    }

    public override void DisplayTournament(UI_Base baseUI, GameObject Container)
    {
        DisplayAsGroupAndTableau(baseUI, Container);
    }

    public override string GetMatchDayTitle(int index)
    {
        return index switch
        {
            0 => "Group Stage Round 1",
            1 => "Group Stage Round 2",
            2 => "Group Stage Round 3",
            3 => "Quarters",
            4 => "Semifinals",
            5 => "Finals",
            _ => ""
        };
    }


    public override Dictionary<int, List<Player>> PlayerRanking => TeamRanking.ToDictionary(x => x.Key, x => GetTeamPlayers(x.Value.First()));
    public override Dictionary<int, List<Team>> TeamRanking
    {
        get
        {
            Dictionary<int, List<Team>> ranking = new Dictionary<int, List<Team>>();
            ranking.Add(0, new List<Team>() { ((TeamMatch)Matches[Matches.Count - 1]).GetTeamRanking()[0].Team });
            ranking.Add(1, new List<Team>() { ((TeamMatch)Matches[Matches.Count - 1]).GetTeamRanking()[1].Team });
            ranking.Add(2, new List<Team>() { ((TeamMatch)Matches[Matches.Count - 2]).GetTeamRanking()[0].Team });
            ranking.Add(3, new List<Team>() { ((TeamMatch)Matches[Matches.Count - 2]).GetTeamRanking()[1].Team });
            return ranking;
        }
    }
}
