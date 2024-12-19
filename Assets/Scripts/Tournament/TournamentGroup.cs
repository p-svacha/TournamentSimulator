using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents a group stage group in a tournament that is played in 1v1 round robin mode. (like world cup or euro)
/// <br/>Participants can either be players or teams. Draws are always allowed in groups.
/// <br/>Each team plays once per day.
/// </summary>
public class TournamentGroup
{
    public Tournament Tournament { get; private set; }
    public string Name { get; private set; }
    public List<int> Participants { get; private set; } // Id's of participants, which can either be players or teams
    public List<Team> ParticipantTeams => Participants.Select(x => Database.Teams[x]).ToList();
    public List<Player> ParticipantPlayers => Participants.Select(x => Database.Players[x]).ToList();
    public List<Match> Matches { get; private set; }
    public bool IsDone => Matches.All(x => x.IsDone);

    /// <summary>
    /// List indices of matches within tournament that players in this group are advancing to. List size dictates number of advancing participants
    /// </summary>
    public List<int> TargetMatchIndices { get; private set; }
    public int NumAdvancements => TargetMatchIndices.Count;

    // Settings
    public int PointsForWin { get; private set; }
    public int PointsForDraw { get; private set; }
    public int PointsForLoss { get; private set; }
    private List<int> PointDistribution = new List<int>() { 1, 0 };

    // Create a new tournament group
    public TournamentGroup(Tournament t, string name, List<int> participants, int pointsForWin, int pointsForDraw, int pointsForLoss, int startDayAbsolute, List<int> targetMatchIndices)
    {
        if (participants.Count != 4) throw new System.Exception("Only group size of 4 is supported atm.");

        Tournament = t;
        Name = name;
        Participants = participants;
        PointsForWin = pointsForWin;
        PointsForDraw = pointsForDraw;
        PointsForLoss = pointsForLoss;
        TargetMatchIndices = targetMatchIndices;

        // Create matches
        Matches = new List<Match>();

        if(Tournament.IsTeamTournament)
        {
            Team[] teams = Participants.Select(x => Database.Teams[x]).ToArray();

            // Day 1
            CreateGroupTeamMatch(round: 1, matchIndex: 1, startDayAbsolute, teams[0], teams[1]);
            CreateGroupTeamMatch(round: 1, matchIndex: 2, startDayAbsolute, teams[2], teams[3]);

            // Day 2
            CreateGroupTeamMatch(round: 2, matchIndex: 1, startDayAbsolute + 1, teams[2], teams[0]);
            CreateGroupTeamMatch(round: 2, matchIndex: 2, startDayAbsolute + 1, teams[3], teams[1]);

            // Day 3
            CreateGroupTeamMatch(round: 3, matchIndex: 1, startDayAbsolute + 2, teams[0], teams[3]);
            CreateGroupTeamMatch(round: 3, matchIndex: 2, startDayAbsolute + 2, teams[1], teams[2]);
        }
        else
        {
            Player[] players = Participants.Select(x => Database.Players[x]).ToArray();

            // Day 1
            CreateGroupMatch(round: 1, matchIndex: 1, startDayAbsolute, players[0], players[1]);
            CreateGroupMatch(round: 1, matchIndex: 2, startDayAbsolute, players[2], players[3]);

            // Day 2
            CreateGroupMatch(round: 2, matchIndex: 1, startDayAbsolute + 1, players[2], players[0]);
            CreateGroupMatch(round: 2, matchIndex: 2, startDayAbsolute + 1, players[3], players[1]);

            // Day 3
            CreateGroupMatch(round: 3, matchIndex: 1, startDayAbsolute + 2, players[0], players[3]);
            CreateGroupMatch(round: 3, matchIndex: 2, startDayAbsolute + 2, players[1], players[2]);
        }
    }

    private void CreateGroupTeamMatch(int round, int matchIndex, int dayAbsolute, Team team1, Team team2)
    {
        int quarter = Database.ToRelativeQuarter(dayAbsolute);
        int day = Database.ToRelativeDay(dayAbsolute);
        
        TeamMatch match = new TeamMatch("Round " + round + " - Match " + matchIndex, Tournament, quarter, day, numTeams: 2, Tournament.NumPlayersPerTeam, PointDistribution, Tournament.GetBasicPointDistribution(Tournament.NumPlayersPerTeam * 2), group: this);
        match.AddTeamToMatch(team1);
        match.AddTeamToMatch(team2);

        Matches.Add(match);
        Tournament.Matches.Add(match);
    }

    private void CreateGroupMatch(int round, int matchIndex, int dayAbsolute, Player p1, Player p2)
    {
        int quarter = Database.ToRelativeQuarter(dayAbsolute);
        int day = Database.ToRelativeDay(dayAbsolute);

        Match match = new FreeForAllMatch("Round " + round + " - Match " + matchIndex, Tournament, quarter, day, numPlayers: 2, PointDistribution, group: this);
        match.AddPlayerToMatch(p1);
        match.AddPlayerToMatch(p2);

        Matches.Add(match);
        Tournament.Matches.Add(match);
    }

    public List<TournamentGroupParticipant> GetGroupLeaderboard()
    {
        if(Tournament.IsTeamTournament)
        {
            Dictionary<Team, TournamentGroupParticipant> leaderboard = new Dictionary<Team, TournamentGroupParticipant>();
            foreach (Team team in ParticipantTeams)
            {
                leaderboard.Add(team, new TournamentGroupParticipant(this, team));
                foreach (TeamMatch m in GetMatchesOf(team))
                {
                    if (!m.IsDone) continue;

                    MatchParticipant_Team teamPart = m.GetParticipant(team);
                    MatchParticipant_Team opponentPart = m.GetOpponent(team);

                    leaderboard[team].NumMatches++;
                    leaderboard[team].TotalMatchPointsGained += teamPart.TotalPoints;
                    leaderboard[team].TotalMatchPointsLost += opponentPart.TotalPoints;

                    if (teamPart.TotalPoints > opponentPart.TotalPoints) leaderboard[team].GroupPoints += PointsForWin;
                    else if(teamPart.TotalPoints == opponentPart.TotalPoints) leaderboard[team].GroupPoints += PointsForDraw;
                    else leaderboard[team].GroupPoints += PointsForLoss;
                }
            }

            List<TournamentGroupParticipant> lbList = leaderboard.Values.OrderByDescending(x => x.GroupPoints).ThenByDescending(x => x.TotalMatchPointRatio).ThenByDescending(x => x.TotalMatchPointsGained).ToList();
            for (int i = 0; i < lbList.Count; i++) lbList[i].Rank = i + 1;
            return lbList;
        }
        else throw new System.Exception("Group leaderboard not yet implemented for individual players, should basically be the same as team.");
    }

    /// <summary>
    /// Gets executed once all matches in a group are done. Advancing teams will be added to the next matches.
    /// </summary>
    public void Conclude()
    {
        // Advancements
        if (Tournament.IsTeamTournament)
        {
            Debug.Log($"Concluding tournament group with {NumAdvancements} advancements.");
            for (int i = 0; i < NumAdvancements; i++)
            {
                int rank = i;
                Team advancingTeam = GetGroupLeaderboard()[rank].Team;
                TeamMatch targetMatch = (TeamMatch)Tournament.Matches[TargetMatchIndices[rank]];
                int seed = rank;
                Debug.Log($"{advancingTeam.Name} is advancing to {targetMatch} as seed {seed}.");
                targetMatch.AddTeamToMatch(advancingTeam, seed: rank);
            }
        }
        else throw new System.Exception("Advancements not yet implemented for individual players, should basically be the same as team.");
    }

    private List<TeamMatch> GetMatchesOf(Team t)
    {
        return Matches.Select(x => (TeamMatch)x).Where(x => x.IncludesTeam(t)).ToList();
    }

    #region Save / Load

    public TournamentGroupData ToData()
    {
        TournamentGroupData data = new TournamentGroupData();
        data.Name = Name;
        data.Participants = Participants;
        data.TargetMatchIndices = TargetMatchIndices;
        data.PointsForWin = PointsForWin;
        data.PointsForDraw = PointsForDraw;
        data.PointsForLoss = PointsForLoss;
        return data;
    }

    public TournamentGroup(Tournament t, TournamentGroupData data)
    {
        Tournament = t;
        Name = data.Name;
        Participants = data.Participants;
        TargetMatchIndices = data.TargetMatchIndices;
        PointsForWin = data.PointsForWin;
        PointsForDraw = data.PointsForDraw;
        PointsForLoss = data.PointsForLoss;

        Matches = new List<Match>();
    }

    #endregion
}
