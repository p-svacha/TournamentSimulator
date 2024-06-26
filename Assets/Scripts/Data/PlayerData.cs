using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int CountryId { get; set; }
    public string Sex { get; set; }
    public int Elo { get; set; }
    public int LeagueType { get; set; }
    public List<PlayerSkillData> Skills { get; set; }
    public float Inconsistency { get; set; }
    public float TiebreakerScore { get; set; }
    public float MistakeChance { get; set; }
}
