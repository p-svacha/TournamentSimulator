using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeamGame : Game
{
    public new TeamMatch Match => (TeamMatch)base.Match;
    public override bool IsTeamGame => true;
    public new List<TeamGameRound> Rounds => base.Rounds.Select(r => (TeamGameRound)r).ToList();

    public TeamGame(Match match, int gameIndex, List<GameModifierDef> gameModifierDefs) : base(match, gameIndex, gameModifierDefs) { }
    public TeamGame(Match match, GameData data) : base(match, data) { }

    public TeamGameRound CreateGameRound(SkillDef skill)
    {
        Dictionary<Player, PlayerGameRound> roundResults = CalculateRoundResult(skill);
        return new TeamGameRound(this, skill, roundResults.Values.ToList());
    }

    /// <summary>
    /// Caluclates team scores according to player points. Then distributes team points according to the ranking that was created based on team scores.
    /// </summary>
    protected override void OnApplyGameRound(GameRound r)
    {
        TeamGameRound round = (TeamGameRound)r;

        // Save
        Rounds.Add(round);
    }

    /// <summary>
    /// The primary rating of the game leaderboard are the points. Points are awarded based on the ranking of the score that each team makes for each skill.
    /// </summary>
    public int GetTeamPoints(MatchParticipant_Team team) => Rounds.Sum(x => x.GetTeamPoints()[team]);

    /// <summary>
    /// Returns the accumulated amount of SCORE a team has gathered throughout the game. Team score is the combined number of POINTS players have made.
    /// </summary>
    public int GetTotalTeamScore(MatchParticipant_Team team) => Rounds.Sum(x => x.GetTeamScores()[team.Team]);

    /// <summary>
    /// Returns the team ranking as a dictionary ordered by end points.
    /// </summary>
    public List<MatchParticipant_Team> GetTeamRanking()
    {
        if (IsDone || IsRunning)
        {
            return Match.TeamParticipants.OrderByDescending(x => GetTeamPoints(x)).ThenByDescending(x => GetTotalTeamScore(x)).ToList();
        }
        else return new List<MatchParticipant_Team>();
    }

   
}
