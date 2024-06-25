using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerListElement : MonoBehaviour
{
    [Header("Elements")]
    public Image Background;
    public Text RankText;
    public Image FlagIcon;
    public Text NameText;
    public Image LeagueIcon;
    public Text ValueText;
    public GameObject MouseOver;
    public Text HoverText;

    private bool IsHovering;

    private void Update()
    {
        if (IsHovering) MouseOver.transform.position = Input.mousePosition + new Vector3(20, 0);
    }

    public void Init(int rank, Player p, string value, Color c, bool showLeagueIcon)
    {
        Background.color = c;
        RankText.text = rank.ToString();
        FlagIcon.sprite = p.FlagSprite;
        HoverText.text = p.Country.Name + " / " + p.Country.Region + " / " + p.Country.Continent;
        NameText.text = p.Name;

        LeagueIcon.sprite = p.LeagueIcon;
        LeagueIcon.color = p.LeagueColor;

        LeagueIcon.gameObject.SetActive(showLeagueIcon);
        ValueText.text = value;

        GetComponent<PlayerTooltipTarget>().Player = p;
    }
}
