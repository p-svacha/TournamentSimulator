using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapDefs
{
    public static List<MapDef> Defs => new List<MapDef>()
    {
        new MapDef()
        {
            DefName="FootballClassic",
            Label = "football",
            Description = "Every football game is played on this map.",
            Skills = new List<SkillDef>()
            {
                SkillDefOf.Agility,
                SkillDefOf.BallControl,
                SkillDefOf.Dribbling,
                SkillDefOf.Jumping,
                SkillDefOf.MentalityGeneral,
                SkillDefOf.Passing,
                SkillDefOf.Positioning,
                SkillDefOf.Shooting,
                SkillDefOf.Sprint,
                SkillDefOf.Stamina,
                SkillDefOf.Strength,
            }
        }
    };
}
