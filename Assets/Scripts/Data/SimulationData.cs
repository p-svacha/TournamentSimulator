using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SimulationData
{
    public int CurrentSeason { get; set; }
    public int CurrentQuarter { get; set; }
    public int CurrentDay { get; set; }
    public List<PlayerData> Players { get; set; }
    public List<LeagueData> Leagues { get; set; }
    public List<TournamentData> Tournaments { get; set; }
    public List<MatchData> Matches { get; set; }
}
