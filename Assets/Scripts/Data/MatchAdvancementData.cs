using System;
using UnityEngine;

[Serializable]
public class MatchAdvancementData
{
    public int SourceRank { get; set; }
    public int TargetMatchId { get; set; }
    public int TargetMatchSeed { get; set; }
}
