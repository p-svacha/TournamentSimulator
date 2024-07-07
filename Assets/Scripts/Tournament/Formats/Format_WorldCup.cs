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

    public Format_WorldCup(TournamentData data) : base(data) { }
    public Format_WorldCup(int season, int playersPerTeam) : base(TournamentType.WorldCup, season)
    {
        NumPlayersPerTeam = playersPerTeam;
        if (NumPlayersPerTeam == 0) NumPlayersPerTeam = 2; // fallback

        Name = "S" + season + " World Cup (" + NumPlayersPerTeam + "v" + NumPlayersPerTeam + ")";

        // Get list of national teams that have enough players
        List<Team> eligibleTeams = Database.GetNationalTeams(minPlayers: NumPlayersPerTeam);

        // Select tournament size (amount of participating teams) based on amount of eligible teams
        int numTeams = -1;
        foreach(int teamAmount in ValidTeamAmounts)
        {
            if (eligibleTeams.Count >= teamAmount) numTeams = teamAmount;
        }
        if (numTeams == -1) throw new System.Exception("Not enough eligible teams to create a world cup with " + NumPlayersPerTeam + " players per team.");

        // Select participants
        Teams = eligibleTeams.OrderByDescending(x => x.GetAveragePlayerElo(NumPlayersPerTeam)).Take(numTeams).ToList();

        Initialize();
    }

    public override void Initialize()
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
        int numGroups = 4;
        int numTeamsPerGroup = 4;

        List<Team> unassignedTeams = new List<Team>();
        unassignedTeams.AddRange(Teams);
        int groupsStartDayAbsolute = Database.ToAbsoluteDay(Season, START_QUARTER, START_DAY);

        // Group phase (match index 0-23)
        for(int i = 0; i < numGroups; i++)
        {
            // Get random teams for group
            List<int> groupParticipants = new List<int>();
            for (int j = 0; j < numTeamsPerGroup; j++)
            {
                Team t = unassignedTeams[Random.Range(0, unassignedTeams.Count)];
                unassignedTeams.Remove(t);
                groupParticipants.Add(t.Id);
            }

            // Calculate target match indices
            List<int> targetMatchIndices = new List<int>() { 24 + (i % 4), 24 + ((i + 1) % 4) };

            // Create group
            TournamentGroup group = new TournamentGroup(this, "Group " + HelperFunctions.GetIndexLetter(i), groupParticipants, POINTS_FOR_WIN, POINTS_FOR_DRAW, POINTS_FOR_LOSS, groupsStartDayAbsolute, targetMatchIndices);
            Groups.Add(group);
        }

        // Quarters (match index 24-27)
        int quartersStartDayAbsolute = groupsStartDayAbsolute + (numTeamsPerGroup - 1);
        int quartersQuarter = Database.ToRelativeQuarter(quartersStartDayAbsolute);
        int quartersDay = Database.ToRelativeDay(quartersStartDayAbsolute);
        for (int i = 0; i < 4; i++)
        {
            TeamMatch quarterfinal = new TeamMatch("Quarterfinal " + (i + 1), this, quartersQuarter, quartersDay, numTeams: 2, NumPlayersPerTeam, TeamPointDistribution, GetBasicPointDistribution(NumPlayersPerTeam * 2));
            quarterfinal.SetTargetMatches(new List<int>() { 28 + (i / 2) });
            Matches.Add(quarterfinal);
        }

        // Semis (match index 28-29)
        int semisStartDayAbsolute = quartersStartDayAbsolute + 1;
        int semisQuarter = Database.ToRelativeQuarter(semisStartDayAbsolute);
        int semisDay = Database.ToRelativeDay(semisStartDayAbsolute);
        for (int i = 0; i < 2; i++)
        {
            TeamMatch semifinal = new TeamMatch("Semifinal " + (i + 1), this, semisQuarter, semisDay, numTeams: 2, NumPlayersPerTeam, TeamPointDistribution, GetBasicPointDistribution(NumPlayersPerTeam * 2));
            semifinal.SetTargetMatches(new List<int>() { 31, 30 });
            Matches.Add(semifinal);
        }

        // Final day
        int finalsDayAbsolute = semisStartDayAbsolute + 1;
        int finalsQuarter = Database.ToRelativeQuarter(finalsDayAbsolute);
        int finalsDay = Database.ToRelativeDay(finalsDayAbsolute);

        // Match for place 3
        Matches.Add(new TeamMatch("Match for place 3", this, finalsQuarter, finalsDay, numTeams: 2, NumPlayersPerTeam, TeamPointDistribution, GetBasicPointDistribution(NumPlayersPerTeam * 2)));

        // Final
        Matches.Add(new TeamMatch("Final", this, finalsQuarter, finalsDay, numTeams: 2, NumPlayersPerTeam, TeamPointDistribution, GetBasicPointDistribution(NumPlayersPerTeam * 2)));
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
}
