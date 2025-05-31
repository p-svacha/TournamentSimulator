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

    public void Init(Player p, SkillDef def)
    {
        LabelText.text = def.Triplet;
        ValueText.text = p.GetSkillBaseValue(def).ToString();
        Background.color = TournamentSimulator.SkillDefs.IndexOf(def) % 2 == 0 ? ColorManager.Singleton.TableListDarkColor : ColorManager.Singleton.TableListLightColor;
    }
}
