using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerMedalListElement : MonoBehaviour
{
    [Header("Elements")]
    public Image Background;
    public Text RankText;
    public Image FlagIcon;
    public Text NameText;
    public Text GoldText;
    public Text SilverText;
    public Text BronzeText;

    private bool IsHovering;

    public void Init(int rank, Player p, Color c, int gold, int silver, int bronze)
    {
        Background.color = c;
        RankText.text = rank.ToString();
        FlagIcon.sprite = p.FlagSmall;
        NameText.text = p.Name;
        GoldText.text = gold.ToString();
        SilverText.text = silver.ToString();
        BronzeText.text = bronze.ToString();

        GetComponent<PlayerTooltipTarget>().Player = p;
    }
}
