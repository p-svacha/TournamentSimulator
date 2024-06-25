using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TournamentData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int LeagueId { get; set; }
    public int Quarter { get; set; }
    public int Day { get; set; }
    public bool IsDone { get; set; }
    public List<int> Players { get; set; }
}
