using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMatchRound
{
    public Player Player { get; private set; }
    public int Score { get; private set; }
    public int PointsGained { get; private set; }
    public List<string> Modifiers { get; private set; }

    public PlayerMatchRound(Player player, int score, List<string> modifiers)
    {
        Player = player;
        Score = score;
        Modifiers = modifiers;
    }

    public void SetPointsGained(int points)
    {
        PointsGained = points;
    }

    #region Save / Load

    public PlayerMatchRoundData ToData()
    {
        PlayerMatchRoundData data = new PlayerMatchRoundData();
        data.PlayerId = Player.Id;
        data.Score = Score;
        data.PointsGained = PointsGained;
        data.Modifiers = Modifiers;
        return data;
    }

    public PlayerMatchRound(PlayerMatchRoundData data)
    {
        Player = Database.Players[data.PlayerId];
        Score = data.Score;
        PointsGained = data.PointsGained;
        Modifiers = data.Modifiers;
    }

    #endregion
}
