using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SkillDefs
{
    /// <summary>
    /// IMPORTANT!
    /// Never remove or rename DefNames of existing SkillDefs, as that would ruin the history and break the game!
    /// </summary>
    public static List<SkillDef> Defs => new List<SkillDef>()
    {
        new SkillDef()
        {
            DefName = "Agility",
            Label = "agility",
            Triplet = "AGI",
        },

        new SkillDef()
        {
            DefName = "BallControl",
            Label = "Ball Control",
            Triplet = "BCT",
        },

        new SkillDef()
        {
            DefName = "Dribbling",
            Label = "Dribbling",
            Triplet = "DRB",
        },

        new SkillDef()
        {
            DefName = "Jumping",
            Label = "Jumping",
            Triplet = "JMP",
        },

        new SkillDef()
        {
            DefName = "MentalityGeneral",
            Label = "Mentality",
            Triplet = "MNT",
        },

        new SkillDef()
        {
            DefName = "Passing",
            Label = "Passing",
            Triplet = "PAS",
        },

        new SkillDef()
        {
            DefName = "Positioning",
            Label = "Positioning",
            Triplet = "POS",
        },

        new SkillDef()
        {
            DefName = "Shooting",
            Label = "Shooting",
            Triplet = "SHO",
        },

        new SkillDef()
        {
            DefName = "Sprint",
            Label = "Sprint",
            Triplet = "SPR",
        },

        new SkillDef()
        {
            DefName = "Stamina",
            Label = "Stamina",
            Triplet = "STM",
        },

        new SkillDef()
        {
            DefName = "Strength",
            Label = "Strength",
            Triplet = "STR",
        },

        new SkillDef()
        {
            DefName = "Gaming",
            Label = "Gaming",
            Triplet = "GAM"
        }
    };
}
