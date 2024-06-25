using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MatchParticipantData
{
    public int PlayerId { get; set; }
    public int TotalScore { get; set; }
    public int EloBeforeMatch { get; set; }
    public int EloAfterMatch { get; set; }
    public int LeaguePointsBeforeMatch { get; set; }
    public List<string> Modifiers { get; set; }
}
