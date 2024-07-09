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

    public void Init(int rank, Player player, Vector3 medals)
    {
        Background.color = ColorManager.Singleton.DefaultColor;
        RankText.text = rank.ToString();
        FlagIcon.sprite = player.FlagSmall;
        NameText.text = player.Name;
        GoldText.text = medals.x.ToString();
        SilverText.text = medals.y.ToString();
        BronzeText.text = medals.z.ToString();

        GetComponent<PlayerTooltipTarget>().Player = player;
    }

    public void Init(int rank, Team team, Vector3 medals)
    {
        Background.color = ColorManager.Singleton.DefaultColor;
        RankText.text = rank.ToString();
        FlagIcon.sprite = team.FlagSmall;
        NameText.text = team.Name;
        GoldText.text = medals.x.ToString();
        SilverText.text = medals.y.ToString();
        BronzeText.text = medals.z.ToString();
    }
}
