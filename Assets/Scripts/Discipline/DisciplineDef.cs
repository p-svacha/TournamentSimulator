using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A discipline represents a very general and broad kind of category. Tournaments are always bound to one specific discipline.
/// A DisciplineDef defines a set of skills that all games of this discipline are competed on.
/// </summary>
public class DisciplineDef : Def
{
    /// <summary>
    /// The set of skills that all games of this disciplines have. More may be added per game with game modifiers.
    /// </summary>
    public List<SkillDef> Skills { get; init; }

    /// <summary>
    /// Set of skills that may optionally appear in some tournaments but are not part of the base discipline.
    /// </summary>
    public List<SkillDef> OptionalSkills { get; init; }
}
