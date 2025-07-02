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
    public Image Background;

    public void Init(Player p, SkillDef def, Color backgroundColor)
    {
        LabelText.text = def.Triplet;
        ValueText.text = p.GetSkillBaseValue(def).ToString();
        Background.color = backgroundColor;
    }
}
