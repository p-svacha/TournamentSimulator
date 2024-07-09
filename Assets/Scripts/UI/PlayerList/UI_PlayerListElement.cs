using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_PlayerListElement : MonoBehaviour
{
    [Header("Elements")]
    public Image Background;
    public TextMeshProUGUI RankText;
    public Image FlagIcon;
    public TextMeshProUGUI NameText;
    public Image LeagueIcon;
    public TextMeshProUGUI ValueText;

    public void Init(int rank, Player p, string value, Color c, bool showLeagueIcon)
    {
        Background.color = c;
        RankText.text = rank.ToString();
        FlagIcon.sprite = p.FlagSmall;
        NameText.text = p.Name;

        LeagueIcon.sprite = p.LeagueIcon;
        LeagueIcon.color = p.LeagueColor;

        LeagueIcon.gameObject.SetActive(showLeagueIcon);
        ValueText.text = value;

        GetComponent<PlayerTooltipTarget>().Player = p;
    }

    public void InitTeamRanking(int rank, Team t)
    {
        Background.color = ColorManager.Singleton.DefaultColor;
        RankText.text = rank.ToString();
        FlagIcon.sprite = t.FlagSmall;
        NameText.text = t.Name;

        LeagueIcon.gameObject.SetActive(false);
        ValueText.text = t.Elo.ToString();
    }
}
