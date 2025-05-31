using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The results of a single game round of a single player. This store the data regardless if it is within a solo or team game round. 
/// </summary>
public class PlayerGameRound
{
    public Player Player { get; private set; }
    public int Score { get; private set; }
    public int PointsGained { get; private set; }
    public List<string> Modifiers { get; private set; }

    public PlayerGameRound(Player player, int score, List<string> modifiers)
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

    public PlayerGameRound(PlayerMatchRoundData data)
    {
        Player = Database.Players[data.PlayerId];
        Score = data.Score;
        PointsGained = data.PointsGained;
        Modifiers = data.Modifiers;
    }

    #endregion
}
