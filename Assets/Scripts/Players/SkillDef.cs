using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillDef
{
    public SkillId Id;
    public string DisplayName;
    public string ThreeLetterDisplay;

    public SkillDef(SkillId id, string displayName, string threeLetterDisplay)
    {
        Id = id;
        DisplayName = displayName;
        ThreeLetterDisplay = threeLetterDisplay;
    }
}
