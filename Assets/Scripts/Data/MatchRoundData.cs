using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MatchRoundData
{
    public int SkillId { get; set; }
    public List<PlayerMatchRoundData> PlayerResults { get; set; } 

}
