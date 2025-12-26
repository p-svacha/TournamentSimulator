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
    public int GroupSize { get; private set; }
    public List<int> Participants { get; private set; } // Id's of participants, which can either be players or teams
    public List<Team> ParticipantTeams => Participants.Select(id => Database.GetTeam(id)).ToList();
    public List<Player> ParticipantPlayers => Participants.Select(id => Database.GetPlayer(id)).ToList();
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
    public TournamentGroup(Tournament t, string name, int groupSize, int pointsForWin, int pointsForDraw, int pointsForLoss, int startDayAbsolute, List<int> targetMatchIndices)
    {
        if (groupSize != 4) throw new System.Exception("Only group size of 4 is supported atm.");

        Tournament = t;
        Name = name;
        GroupSize = groupSize;
        Participants = new List<int>();
        PointsForWin = pointsForWin;
        PointsForDraw = pointsForDraw;
        PointsForLoss = pointsForLoss;
        TargetMatchIndices = targetMatchIndices;

        // Create matches
        Matches = new List<Match>();

        if(Tournament.IsTeamTournament)
        {
            Team[] teams = Participants.Select(id => Database.GetTeam(id)).ToArray();

            // Day 1
            CreateGroupTeamMatch(round: 1, matchIndex: 1, startDayAbsolute);
            CreateGroupTeamMatch(round: 1, matchIndex: 2, startDayAbsolute);

            // Day 2
            CreateGroupTeamMatch(round: 2, matchIndex: 1, startDayAbsolute + 1);
            CreateGroupTeamMatch(round: 2, matchIndex: 2, startDayAbsolute + 1);

            // Day 3
            CreateGroupTeamMatch(round: 3, matchIndex: 1, startDayAbsolute + 2);
            CreateGroupTeamMatch(round: 3, matchIndex: 2, startDayAbsolute + 2);
        }
        else
        {
            Player[] players = Participants.Select(id => Database.GetPlayer(id)).ToArray();

            // Day 1
            CreateGroupMatch(round: 1, matchIndex: 1, startDayAbsolute);
            CreateGroupMatch(round: 1, matchIndex: 2, startDayAbsolute);

            // Day 2
            CreateGroupMatch(round: 2, matchIndex: 1, startDayAbsolute + 1);
            CreateGroupMatch(round: 2, matchIndex: 2, startDayAbsolute + 1);

            // Day 3
            CreateGroupMatch(round: 3, matchIndex: 1, startDayAbsolute + 2);
            CreateGroupMatch(round: 3, matchIndex: 2, startDayAbsolute + 2);
        }
    }

    /// <summary>
    /// Sets the teams in this group according to the seeding in the given list and adds them to the according matches.
    /// </summary>
    public void SetParticipants(List<Team> teams)
    {
        if (!Tournament.IsTeamTournament) throw new System.Exception("Can't add teams to a group of a solo tournament.");
        if (teams.Count != GroupSize) throw new System.Exception($"Number of teams must match group size. (expected: {GroupSize}, received {teams.Count})");

        Participants = teams.Select(t => t.Id).ToList();

        ((TeamMatch)Matches[0]).AddTeamToMatch(teams[0]);
        ((TeamMatch)Matches[0]).AddTeamToMatch(teams[3]);
        ((TeamMatch)Matches[1]).AddTeamToMatch(teams[1]);
        ((TeamMatch)Matches[1]).AddTeamToMatch(teams[2]);

        ((TeamMatch)Matches[2]).AddTeamToMatch(teams[2]);
        ((TeamMatch)Matches[2]).AddTeamToMatch(teams[0]);
        ((TeamMatch)Matches[3]).AddTeamToMatch(teams[3]);
        ((TeamMatch)Matches[3]).AddTeamToMatch(teams[1]);

        ((TeamMatch)Matches[4]).AddTeamToMatch(teams[0]);
        ((TeamMatch)Matches[4]).AddTeamToMatch(teams[1]);
        ((TeamMatch)Matches[5]).AddTeamToMatch(teams[2]);
        ((TeamMatch)Matches[5]).AddTeamToMatch(teams[3]);
    }

    /// <summary>
    /// Sets the players in this group according to the seeding in the given list and adds them to the according matches.
    /// </summary>
    public void SetParticipants(List<Player> players)
    {
        if (Tournament.IsTeamTournament) throw new System.Exception("Can't add players to a group of a team tournament.");
        if (players.Count != GroupSize) throw new System.Exception($"Number of players must match group size. (expected: {GroupSize}, received {players.Count})");

        Participants = players.Select(t => t.Id).ToList();

        Matches[0].AddPlayerToMatch(players[0]);
        Matches[0].AddPlayerToMatch(players[3]);
        Matches[1].AddPlayerToMatch(players[1]);
        Matches[1].AddPlayerToMatch(players[2]);

        Matches[2].AddPlayerToMatch(players[2]);
        Matches[2].AddPlayerToMatch(players[0]);
        Matches[3].AddPlayerToMatch(players[3]);
        Matches[3].AddPlayerToMatch(players[1]);

        Matches[4].AddPlayerToMatch(players[0]);
        Matches[4].AddPlayerToMatch(players[1]);
        Matches[5].AddPlayerToMatch(players[2]);
        Matches[5].AddPlayerToMatch(players[3]);
    }

    private void CreateGroupTeamMatch(int round, int matchIndex, int dayAbsolute)
    {
        int quarter = Database.ToRelativeQuarter(dayAbsolute);
        int day = Database.ToRelativeDay(dayAbsolute);
        
        TeamMatch match = new TeamMatch("Round " + round + " - Match " + matchIndex, Tournament, quarter, day, MatchFormatDefOf.SingleGame, numTeams: 2, Tournament.NumPlayersPerTeam, PointDistribution, Tournament.GetBasicPointDistribution(Tournament.NumPlayersPerTeam * 2), group: this);

        Matches.Add(match);
        Tournament.Matches.Add(match);
    }

    private void CreateGroupMatch(int round, int matchIndex, int dayAbsolute)
    {
        int quarter = Database.ToRelativeQuarter(dayAbsolute);
        int day = Database.ToRelativeDay(dayAbsolute);

        Match match = new SoloMatch("Round " + round + " - Match " + matchIndex, Tournament, quarter, day, MatchFormatDefOf.SingleGame, maxPlayers: 2, PointDistribution, group: this);

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

                    MatchParticipant_Team teamParticipant = m.GetParticipant(team);
                    MatchParticipant_Team opponentParticipant = m.GetOpponent(team);

                    int teamMatchScore = m.GetTeamMatchScore(teamParticipant);
                    int oppMatchScore = m.GetTeamMatchScore(opponentParticipant);

                    leaderboard[team].NumMatches++;
                    leaderboard[team].TotalMatchPointsGained += teamMatchScore;
                    leaderboard[team].TotalMatchPointsLost += oppMatchScore;

                    if (teamMatchScore > oppMatchScore) leaderboard[team].GroupPoints += PointsForWin;
                    else if(teamMatchScore == oppMatchScore) leaderboard[team].GroupPoints += PointsForDraw;
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
        data.Size = GroupSize;
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
        GroupSize = data.Size;
        Participants = data.Participants;
        TargetMatchIndices = data.TargetMatchIndices;
        PointsForWin = data.PointsForWin;
        PointsForDraw = data.PointsForDraw;
        PointsForLoss = data.PointsForLoss;

        Matches = new List<Match>();
    }

    #endregion
}
