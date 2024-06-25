using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerMatchRoundData
{
    public int PlayerId { get; set; }
    public int Score { get; set; }
    public int PointsGained { get; set; }
    public List<string> Modifiers { get; set; }
}
