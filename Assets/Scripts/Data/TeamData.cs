using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TeamData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<int> Players { get; set; }
    public int CountryId { get; set; }
    public ColorData Color1 { get; set; }
    public ColorData Color2 { get; set; }
    public List<EloData> Elos { get; set; }
}
