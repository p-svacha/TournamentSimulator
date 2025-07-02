using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSkillData
{
    public string Skill { get; set; }
    public float BaseValue { get; set; }
    public float Inconsistency { get; set; }
    public float MistakeChance { get; set; }
}
