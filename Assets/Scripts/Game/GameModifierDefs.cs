using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameModifierDefs
{
    public static List<GameModifierDef> Defs => new List<GameModifierDef>()
    {
        new GameModifierDef()
        {
            DefName = "BIGCup",
            Skills = new List<SkillDef>() { SkillDefOf.Obstacle },
        }
    };
}
