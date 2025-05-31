using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Each game is played on a specific map. The map defines which skills are active within that game.
/// </summary>
public class MapDef : Def
{
    /// <summary>
    /// The set of skills that are relevant for this map.
    /// </summary>
    public List<SkillDef> Skills { get; init; } = new();
}
