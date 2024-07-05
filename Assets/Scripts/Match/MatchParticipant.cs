using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchParticipant
{
    public Player Player { get; private set; }
    public int Seed { get; private set; }
    public Team Team { get; private set; }
    public int TotalPoints { get; private set; }
    public int EloBeforeMatch { get; private set; }
    public int EloAfterMatch { get; private set; }
    public int LeaguePointsBeforeMatch { get; private set; }
    public List<string> Modifiers { get; private set; }

    public MatchParticipant(Player player, int seed, Team team)
    {
        Player = player;
        Seed = seed;
        Team = team;
    }

    public void IncreaseTotalPoints(int points)
    {
        TotalPoints += points;
    }
    public void SetPreMatchStats()
    {
        EloBeforeMatch = Player.Elo;
        LeaguePointsBeforeMatch = Player.League == null ? -1 : Player.CurrentLeaguePoints;
    }
    public void SetEloAfterMatch(int eloAfterMatch)
    {
        EloAfterMatch = eloAfterMatch;
    }


    #region Save / Load

    public MatchParticipantData ToData()
    {
        MatchParticipantData data = new MatchParticipantData();
        data.PlayerId = Player.Id;
        data.Seed = Seed;
        data.Team = Team == null ? - 1 : Team.Id;
        data.TotalScore = TotalPoints;
        data.EloBeforeMatch = EloBeforeMatch;
        data.EloAfterMatch = EloAfterMatch;
        data.LeaguePointsBeforeMatch = LeaguePointsBeforeMatch;
        data.Modifiers = Modifiers;
        return data;
    }

    public MatchParticipant(MatchParticipantData data)
    {
        Player = Database.Players[data.PlayerId];
        Seed = data.Seed;
        Team = data.Team == -1 ? null : Database.Teams[data.Team];
        TotalPoints = data.TotalScore;
        EloBeforeMatch = data.EloBeforeMatch;
        EloAfterMatch = data.EloAfterMatch;
        LeaguePointsBeforeMatch = data.LeaguePointsBeforeMatch;
        Modifiers = data.Modifiers;
    }

    #endregion
}
