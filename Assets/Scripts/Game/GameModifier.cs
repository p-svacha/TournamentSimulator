using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A game modifier adds a set of skills to be competed on in a game.
/// </summary>
public class GameModifier
{
    public string Label { get; private set; }
    public List<SkillDef> Skills { get; private set; }

    public GameModifier(GameModifierDef def)
    {
        Label = def.Label;
        Skills = new List<SkillDef>(def.Skills);
    }

    #region Load / Save

    public GameModifierData ToData()
    {
        GameModifierData data = new GameModifierData();

        data.Label = Label;
        data.SkillsAdded = Skills.Select(s => s.DefName).ToList();

        return data;
    }

    public GameModifier(GameModifierData data)
    {
        Label = data.Label;
        Skills = data.SkillsAdded.Select(s => DefDatabase<SkillDef>.GetNamed(s)).ToList();
    }

    #endregion
}
