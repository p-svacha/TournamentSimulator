using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TournamentGroupData
{
    public string Name { get; set; }
    public int PointsForWin { get; set; }
    public int PointsForDraw { get; set; }
    public int PointsForLoss { get; set; }
    public List<int> Participants { get; set; }
    public List<int> TargetMatchIndices { get; set; }
}
