using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillDef
{
    public SkillId Id;
    public string DefName;
    public string Label;
    public string Triplet;

    public SkillDef(SkillId id, string defName, string label, string triplet)
    {
        Id = id;
        DefName = defName;
        Label = label;
        Triplet = triplet;
    }
}
