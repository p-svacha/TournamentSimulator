using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeamGameRound : GameRound
{
    public TeamGame Game => (TeamGame)game;
    public TeamMatch Match => Game.Match;
    public TeamGameRound(TeamGame game, SkillDef skill, List<PlayerGameRound> playerResults) : base(game, skill, playerResults) { }
    public TeamGameRound(TeamGame game, GameRoundData data) : base(game, data) { }

    /// <summary>
    /// Returns the score that each team achieved in this round in an ordered dictionary. The score is the cumulative amount of POINTS (not score!) of every player of that team.
    /// </summary>
    public Dictionary<Team, int> GetTeamScores()
    {
        Dictionary<Team, int> teamScores = new Dictionary<Team, int>();
        foreach (MatchParticipant_Team teamParticipant in Game.Match.TeamParticipants) teamScores.Add(teamParticipant.Team, 0);

        foreach (PlayerGameRound playerResult in PlayerResults)
        {
            Team team = Game.Match.GetTeamOf(playerResult.Player);
            int playerPoints = playerResult.PointsGained;
            teamScores[team] += playerPoints;
        }

        return teamScores.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
    }

    /// <summary>
    /// Returns the points that each team achieved in this round in an ordered dictionary. The points are awarded based on team score ranking.
    /// </summary>
    public Dictionary<MatchParticipant_Team, int> GetTeamPoints()
    {
        Dictionary<MatchParticipant_Team, int> teamPoints = new Dictionary<MatchParticipant_Team, int>();
        foreach (MatchParticipant_Team teamParticipant in Match.TeamParticipants) teamPoints.Add(teamParticipant, 0);

        // Add to team points
        Dictionary<Team, int> teamScores = GetTeamScores();
        List<Team> teamRanking = teamScores.Keys.Reverse().ToList();

        int lastScore = -1;
        int lastPoints = -1;
        for (int rank = 0; rank < teamScores.Count; rank++)
        {
            Team team = teamRanking[rank];
            int score = teamScores[team];
            int pointsGained = Match.TeamPointDistribution[Match.TeamPointDistribution.Count - rank - 1];
            if (score == 0) pointsGained = 0;
            else if (score == lastScore) pointsGained = lastPoints;
            MatchParticipant_Team participant = Match.GetParticipant(team);
            teamPoints[participant] += pointsGained;
            lastScore = score;
            lastPoints = pointsGained;
        }

        return teamPoints;
    }
}
