using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MatchParticipantData
{
    public int PlayerId { get; set; }
    public int Seed { get; set; }
    public int Team { get; set; }
    public int EloBeforeMatch { get; set; }
    public int EloAfterMatch { get; set; }
    public int LeaguePointsBeforeMatch { get; set; }
}
