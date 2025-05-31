using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MatchRoundData
{
    public string Skill { get; set; }
    public List<PlayerMatchRoundData> PlayerResults { get; set; } 

}
