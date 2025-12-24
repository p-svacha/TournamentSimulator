using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TournamentData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Discipline { get; set; }
    public DisciplineData DisciplineInfo { get; set; }
    public int Format { get; set; }
    public int LeagueId { get; set; }
    public int Season { get; set; }
    public bool IsDone { get; set; }
    public List<int> Players { get; set; }
    public List<TournamentGroupData> Groups { get; set; }
    public List<int> Teams { get; set; }
    public int NumPlayersPerTeam { get; set; }
    public List<string> ModifierDefs { get; set; }
}
