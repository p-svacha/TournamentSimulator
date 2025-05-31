using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public int Index { get; set; }
    public List<GameRoundData> Rounds { get; set; }
}
