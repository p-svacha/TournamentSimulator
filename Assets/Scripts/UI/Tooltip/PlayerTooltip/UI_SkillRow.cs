using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_SkillRow : MonoBehaviour
{
    [Header("Elements")]
    public TextMeshProUGUI LabelText;
    public TextMeshProUGUI ValueText;
    public TextMeshProUGUI InconsistencyText;
    public TextMeshProUGUI MistakeChanceText;
    public Image Background;

    public void Init(Player p, Skill skill, Color backgroundColor)
    {
        LabelText.text = skill.Def.Triplet;
        ValueText.text = Mathf.RoundToInt(skill.BaseValue).ToString();
        InconsistencyText.text = "±" + Mathf.RoundToInt(skill.Inconsistency).ToString();
        MistakeChanceText.text = skill.MistakeChance.ToString("P0");
        Background.color = backgroundColor;
    }
}
