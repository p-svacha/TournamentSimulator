using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_GroupPlayer : MonoBehaviour
{
    [Header("Elements")]
    public Image Background;
    public Image FlagIcon;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI EloText;
    public Image EloChangeIcon;
    public TextMeshProUGUI EloChangeText;
    public TextMeshProUGUI PointsText;

    public void Init(Match match, MatchParticipant p)
    {
        FlagIcon.sprite = p.Player.FlagSprite;
        NameText.text = p.Player.Name;
        if(match.IsDone)
        {
            EloText.text = p.EloBeforeMatch.ToString();
            PointsText.text = p.TotalScore.ToString();

            int eloChange = p.EloAfterMatch - p.EloBeforeMatch;
            if(eloChange > 0)
            {
                EloChangeIcon.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 90);
                EloChangeIcon.color = ColorManager.Singleton.GreenTextColor;
                EloChangeText.text = "+" + eloChange;
                EloChangeText.color = ColorManager.Singleton.GreenTextColor;
            }
            else if(eloChange < 0)
            {
                EloChangeIcon.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, -90);
                EloChangeIcon.color = ColorManager.Singleton.RedTextColor;
                EloChangeText.text = eloChange.ToString();
                EloChangeText.color = ColorManager.Singleton.RedTextColor;
            }
            else
            {
                EloChangeIcon.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
                EloChangeText.text = "±0";
            }

            int rank = match.PlayerRanking.IndexOf(p.Player);
            if(match.NumAdvancements == 0) Background.color = ColorManager.Singleton.DefaultColor;
            else if (rank < match.NumAdvancements) Background.color = ColorManager.Singleton.AdvanceColor;
            else Background.color = ColorManager.Singleton.KoColor;
        }
        else
        {
            EloText.text = p.Player.Elo.ToString();
            EloChangeIcon.enabled = false;
            EloChangeText.text = p.Player.CurrentLeaguePoints.ToString();
            PointsText.text = "";
            Background.color = ColorManager.Singleton.DefaultColor;
        }

        GetComponent<PlayerTooltipTarget>().Player = p.Player;
    }
}
