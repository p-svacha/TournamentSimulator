using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchParticipant_Team
{
    public TeamMatch Match { get; private set; }
    public Team Team { get; private set; }
    public int Seed { get; private set; }
    public int MatchScore { get; private set; }
    public int EloBeforeMatch { get; private set; }
    public int EloAfterMatch { get; private set; }

    public MatchParticipant_Team(TeamMatch match, Team team, int seed)
    {
        Match = match;
        Team = team;
        Seed = seed;
    }

    public void SetPreMatchStats()
    {
        EloBeforeMatch = Team.Elo[Match.Discipline.Def];
    }
    public void SetEloAfterMatch(int eloAfterMatch)
    {
        EloAfterMatch = eloAfterMatch;
    }

    #region Save / Load

    public TeamMatchParticipantData ToData()
    {
        TeamMatchParticipantData data = new TeamMatchParticipantData();
        data.TeamId = Team.Id;
        data.Seed = Seed;
        data.MatchScore = MatchScore;
        data.EloBeforeMatch = EloBeforeMatch;
        data.EloAfterMatch = EloAfterMatch;
        return data;
    }

    public MatchParticipant_Team(TeamMatch match, TeamMatchParticipantData data)
    {
        Match = match;
        Team = Database.GetTeam(data.TeamId);
        Seed = data.Seed;
        MatchScore = data.MatchScore;
        EloBeforeMatch = data.EloBeforeMatch;
        EloAfterMatch = data.EloAfterMatch;
    }

    #endregion
}
