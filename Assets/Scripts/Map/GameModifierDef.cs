using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Each game can have a set of game modifiers, that add additional skills to the game.
/// </summary>
public class GameModifierDef : Def
{
    /// <summary>
    /// The set of skills that are added for this modifier.
    /// </summary>
    public List<SkillDef> Skills { get; init; } = new();
}
