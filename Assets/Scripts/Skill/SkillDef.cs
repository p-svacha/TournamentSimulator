using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Each player in the database has numerical value for each SkillDef, defining how good they are at that skill.
/// </summary>
public class SkillDef : Def
{
    /// <summary>
    /// The three letter abbreviation of the skill.
    /// </summary>
    public string Triplet { get; init; }
}
