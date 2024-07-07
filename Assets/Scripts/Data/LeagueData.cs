using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LeagueData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Season { get; set; }
    public int LeagueType { get; set; }
    public List<LeagueParticipantData> Participants { get; set; }
    public int NumPromotions { get; set; }
    public int NumRelegations { get; set; }
}
