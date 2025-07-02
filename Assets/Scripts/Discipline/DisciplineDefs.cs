using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DisciplineDefs
{
    public static List<DisciplineDef> Defs => new List<DisciplineDef>()
    {
        new DisciplineDef()
        {
            DefName = "Football",
            Label = "Football",
            Description = "The classic football game.",
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
        },

        new DisciplineDef()
        {
            DefName = "AgeOfEmpires",
            Label = "Age of Empires",
            Description = "Age of Empires II"
        },
    };
}
