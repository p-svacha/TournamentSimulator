using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchParticipant_Team
{
    public Team Team { get; private set; }
    public int Seed { get; private set; }
    public int TotalPoints { get; private set; }
    public int EloBeforeMatch { get; private set; }
    public int EloAfterMatch { get; private set; }
    public List<string> Modifiers { get; private set; }

    public MatchParticipant_Team(Team team, int seed)
    {
        Team = team;
        Seed = seed;
    }

    public void IncreaseTotalPoints(int points)
    {
        TotalPoints += points;
    }

    public void SetPreMatchStats()
    {
        EloBeforeMatch = Team.Elo;
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
        data.TotalScore = TotalPoints;
        data.EloBeforeMatch = EloBeforeMatch;
        data.EloAfterMatch = EloAfterMatch;
        data.Modifiers = Modifiers;
        return data;
    }

    public MatchParticipant_Team(TeamMatchParticipantData data)
    {
        Team = Database.Teams[data.TeamId];
        Seed = data.Seed;
        TotalPoints = data.TotalScore;
        EloBeforeMatch = data.EloBeforeMatch;
        EloAfterMatch = data.EloAfterMatch;
        Modifiers = data.Modifiers;
    }

    #endregion
}
