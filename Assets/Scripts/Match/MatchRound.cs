using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchRound
{
    private Match Match;
    public SkillId SkillId { get; private set; }
    public List<PlayerMatchRound> PlayerResults { get; private set; }

    public MatchRound (Match match, SkillId skillId, List<PlayerMatchRound> playerResults)
    {
        Match = match;
        SkillId = skillId;
        PlayerResults = playerResults;
    }

    public PlayerMatchRound GetPlayerResult(Player p) => PlayerResults.First(x => x.Player == p);
    public List<Player> PlayerRanking => PlayerResults.OrderByDescending(x => x.Score).ThenByDescending(x => x.Player.TiebreakerScore).Select(x => x.Player).ToList();


    /// <summary>
    /// Returns the score that each team achieved in this round in an ordered dictionary. The score is the cumulative amount of POINTS (not score!) of every player of that team.
    /// </summary>
    public Dictionary<Team, int> GetTeamScores()
    {
        if (!(Match is TeamMatch teamMatch)) throw new System.Exception("Can't get team scores of a match that is not a team match.");

        Dictionary<Team, int> teamScores = new Dictionary<Team, int>();
        foreach (MatchParticipant_Team teamParticipant in teamMatch.TeamParticipants) teamScores.Add(teamParticipant.Team, 0);

        foreach(PlayerMatchRound playerResult in PlayerResults)
        {
            Team team = Match.GetTeamOf(playerResult.Player);
            int playerPoints = playerResult.PointsGained;
            teamScores[team] += playerPoints;
        }

        return teamScores.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
    }

    #region Save / Load

    public MatchRoundData ToData()
    {
        MatchRoundData data = new MatchRoundData();
        data.SkillId = (int)SkillId;
        data.PlayerResults = PlayerResults.Select(x => x.ToData()).ToList();
        return data;
    }

    public MatchRound(Match match, MatchRoundData data)
    {
        Match = match;
        SkillId = (SkillId)data.SkillId;
        PlayerResults = data.PlayerResults.Select(x => new PlayerMatchRound(x)).ToList();
    }

    #endregion
}
