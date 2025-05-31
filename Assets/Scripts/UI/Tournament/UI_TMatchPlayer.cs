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

    public void Init(Match match, MatchParticipant_Player p, bool isCompact)
    {
        int currentLeaguePoints = (!match.IsDone && match.Tournament.League != null) ? p.Player.CurrentLeaguePoints : 0;

        if (isCompact) InitCompact(match, p.Player.FlagSmall, p.Player.LastName, match.PlayerParticipantRanking.IndexOf(p), p.MatchScore);
        else InitFull(match, p.Player.FlagSmall, p.Player.Name, match.PlayerParticipantRanking.IndexOf(p), p.MatchScore, p.Player.Elo, currentLeaguePoints, p.EloBeforeMatch, p.EloAfterMatch);

        GetComponent<PlayerTooltipTarget>().Player = p.Player;
    }

    public void Init(TeamMatch match, MatchParticipant_Team t, bool isCompact)
    {
        if (isCompact) InitCompact(match, t.Team.FlagSmall, t.Team.Name, match.TeamRanking.IndexOf(t), t.MatchScore);
        else InitFull(match, t.Team.FlagSmall, t.Team.Name, match.TeamRanking.IndexOf(t), t.MatchScore, t.Team.Elo, 0, t.EloBeforeMatch, t.EloAfterMatch);
    }

    public void InitEmpty()
    {
        FlagIcon.enabled = false;
        NameText.text = "";
        if (EloText != null) EloText.text = "";
        if (EloChangeIcon != null) EloChangeIcon.enabled = false;
        if (EloChangeText != null) EloChangeText.text = "";
        if (PointsText != null) PointsText.text = "";
        return;
    }

    private void InitCompact(Match match, Sprite sprite, string name, int rank, int points)
    {
        FlagIcon.sprite = sprite;
        NameText.text = name;
        Background.color = GetBackgroundColor(match, rank);

        if (match.IsDone) PointsText.text = points.ToString();
        else PointsText.text = "";
    }
    private void InitFull(Match match, Sprite sprite, string name, int rank, int points, int currentElo, int currentLP, int eloBeforeMatch, int eloAfterMatch)
    {
        FlagIcon.sprite = sprite;
        NameText.text = name;
        Background.color = GetBackgroundColor(match, rank);

        if (match.IsDone)
        {
            PointsText.text = points.ToString();

            EloText.text = eloBeforeMatch.ToString();
            int eloChange = eloAfterMatch - eloBeforeMatch;
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
        else
        {
            PointsText.text = "";
            EloChangeIcon.enabled = false;
            EloText.text = currentElo.ToString();
            EloChangeText.text = match.Tournament.League != null ? currentLP.ToString() : "";
        }
    }

    private Color GetBackgroundColor(Match match, int rank)
    {
        if (match.IsDone)
        {
            if (match.NumParticipants == 2) // Always color winner green and loser red in finished matches with 2 participants
            {
                if (rank == 0) return ColorManager.Singleton.AdvanceColor;
                else return ColorManager.Singleton.KoColor;
            }

            else if (match.NumAdvancements == 0) return ColorManager.Singleton.DefaultColor; // Gray if there are no advancements in this match
            else if (rank < match.NumAdvancements) return ColorManager.Singleton.AdvanceColor; // Green if within advancements
            else return ColorManager.Singleton.KoColor; // Red if not within advancements
        }
        else return ColorManager.Singleton.DefaultColor; // Gray if match is not done
    }
}
