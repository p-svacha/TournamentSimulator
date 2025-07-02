using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeamGame : Game
{
    public new TeamMatch Match => (TeamMatch)base.Match;
    public override bool IsTeamGame => true;
    private Dictionary<MatchParticipant_Team, int> TeamPoints;
    public new List<TeamGameRound> Rounds => base.Rounds.Select(r => (TeamGameRound)r).ToList();

    public TeamGame(Match match, int gameIndex, List<GameModifierDef> gameModifierDefs) : base(match, gameIndex, gameModifierDefs) { }
    public TeamGame(Match match, GameData data) : base(match, data) { }

    protected override void OnStartGame()
    {
        TeamPoints = new Dictionary<MatchParticipant_Team, int>();
        foreach (MatchParticipant_Team participant in Match.TeamParticipants) TeamPoints.Add(participant, 0);
    }

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

        // Add to team points
        Dictionary<Team, int> teamScores = round.GetTeamScores();
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
            TeamPoints[participant] += pointsGained;
            lastScore = score;
            lastPoints = pointsGained;
        }

        // Save
        Rounds.Add(round);
    }

    /// <summary>
    /// Returns the accumulated amount of SCORE a team has gathered throughout the game. Team score is the combined number of POINTS players have made.
    /// </summary>
    public int GetTotalTeamScore(Team team) => Rounds.Sum(x => x.GetTeamScores()[team]);

    /// <summary>
    /// Returns the team ranking as a dictionary ordered by end points.
    /// </summary>
    public List<MatchParticipant_Team> TeamRanking => IsDone ? TeamPoints.OrderByDescending(x => x.Value).ThenByDescending(x => GetTotalTeamScore(x.Key.Team)).Select(x => x.Key).ToList() : new();
}
