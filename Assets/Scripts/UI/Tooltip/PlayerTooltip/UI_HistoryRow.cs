using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_HistoryRow : MonoBehaviour
{
    [Header("Elements")]
    public TextMeshProUGUI SeasonText;
    public Image LeagueImage;
    public TextMeshProUGUI RankText;

    public void Init(Player p, League league)
    {
        SeasonText.text = "S" + league.Season;
        LeagueImage.sprite = league.Icon;
        LeagueImage.color = league.Color;
        RankText.text = league.GetRankOf(p) + " / " + league.Players.Count;
    }
}
