using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchParticipant
{
    public Player Player { get; private set; }
    public int TotalScore { get; private set; }
    public int EloBeforeMatch { get; private set; }
    public int EloAfterMatch { get; private set; }
    public int LeaguePointsBeforeMatch { get; private set; }
    public List<string> Modifiers { get; private set; }

    public MatchParticipant(Player player)
    {
        Player = player;
    }
    public MatchParticipant(Player player, int totalScore, int eloBeforeMatch, int eloAfterMatch, int leaguePointsBeforeMatch)
    {
        Player = player;
        TotalScore = totalScore;
        EloBeforeMatch = eloBeforeMatch;
        EloAfterMatch = eloAfterMatch;
        LeaguePointsBeforeMatch = leaguePointsBeforeMatch;
    }

    public void IncreaseTotalScore(int totalScore)
    {
        TotalScore += totalScore;
    }
    public void SetPreMatchStats()
    {
        EloBeforeMatch = Player.Elo;
        LeaguePointsBeforeMatch = Player.CurrentLeaguePoints;
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
        data.TotalScore = TotalScore;
        data.EloBeforeMatch = EloBeforeMatch;
        data.EloAfterMatch = EloAfterMatch;
        data.LeaguePointsBeforeMatch = LeaguePointsBeforeMatch;
        data.Modifiers = Modifiers;
        return data;
    }

    public MatchParticipant(MatchParticipantData data)
    {
        Player = Database.Players[data.PlayerId];
        TotalScore = data.TotalScore;
        EloBeforeMatch = data.EloBeforeMatch;
        EloAfterMatch = data.EloAfterMatch;
        LeaguePointsBeforeMatch = data.LeaguePointsBeforeMatch;
        Modifiers = data.Modifiers;
    }

    #endregion
}
