using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeamGameRound : GameRound
{
    public TeamGame Game => (TeamGame)game;
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
}
