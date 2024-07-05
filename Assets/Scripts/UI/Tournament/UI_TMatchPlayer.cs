using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Display of a player in a match in tournament view
/// </summary>
public class UI_TMatchPlayer : MonoBehaviour
{
    [Header("Elements")]
    public Image Background;
    public Image FlagIcon;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI EloText;
    public Image EloChangeIcon;
    public TextMeshProUGUI EloChangeText;
    public TextMeshProUGUI PointsText;

    public void Init(Match match, MatchParticipant p, bool isCompact)
    {
        if(p == null)
        {
            FlagIcon.enabled = false;
            NameText.text = "";
            if (EloText != null) EloText.text = "";
            if (EloChangeIcon != null) EloChangeIcon.enabled = false;
            if (EloChangeText != null) EloChangeText.text = "";
            if (PointsText != null) PointsText.text = "";
            return;
        }

        FlagIcon.sprite = p.Player.FlagSprite;
        NameText.text = isCompact ? p.Player.LastName : p.Player.Name;
        if(match.IsDone)
        {
            PointsText.text = p.TotalPoints.ToString();

            int rank = match.PlayerRanking.IndexOf(p.Player);
            if(match.NumAdvancements == 0) Background.color = ColorManager.Singleton.DefaultColor;
            else if (rank < match.NumAdvancements) Background.color = ColorManager.Singleton.AdvanceColor;
            else Background.color = ColorManager.Singleton.KoColor;

            if (!isCompact)
            {
                EloText.text = p.EloBeforeMatch.ToString();
                int eloChange = p.EloAfterMatch - p.EloBeforeMatch;
                if (eloChange > 0)
                {
                    EloChangeIcon.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 90);
                    EloChangeIcon.color = ColorManager.Singleton.GreenTextColor;
                    EloChangeText.text = "+" + eloChange;
                    EloChangeText.color = ColorManager.Singleton.GreenTextColor;
                }
                else if (eloChange < 0)
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
            }
        }
        else
        {
            PointsText.text = "";
            Background.color = ColorManager.Singleton.DefaultColor;

            if (!isCompact)
            {
                EloChangeIcon.enabled = false;
                EloText.text = p.Player.Elo.ToString();
                EloChangeText.text = match.Tournament.League != null ? p.Player.CurrentLeaguePoints.ToString() : "";
            }
        }

        GetComponent<PlayerTooltipTarget>().Player = p.Player;
    }
}
