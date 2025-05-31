using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TeamMatchParticipantData
{
    public int TeamId { get; set; }
    public int Seed { get; set; }
    public int MatchScore { get; set; }
    public int EloBeforeMatch { get; set; }
    public int EloAfterMatch { get; set; }
}
