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
    public TextMeshProUGUI PlaceholderText;

    public void Init(Match match, MatchParticipant_Player p, bool isCompact)
    {
        if (isCompact) InitCompact(match, p.Player.FlagSmall, p.Player.LastName, match.GetPlayerRanking().IndexOf(p), match.IsDone ? match.GetPlayerMatchScore(p).ToString() : "");
        else InitFullPlayer(match, p);

        GetComponent<PlayerTooltipTarget>().Init(match.Discipline.Def, p.Player);
    }

    public void Init(TeamMatch match, MatchParticipant_Team t, bool isCompact)
    {
        if (isCompact) InitCompact(match, t.Team.FlagSmall, t.Team.Name, match.GetTeamRanking().IndexOf(t), match.IsDone ? match.GetTeamMatchScore(t).ToString() : "");
        else InitFullTeam(match, t);

        GetComponent<TeamTooltipTarget>().Init(match.Discipline.Def, t.Team);
    }

    public void InitEmpty(string text = "")
    {
        FlagIcon.enabled = false;
        NameText.text = "";
        if (EloText != null) EloText.text = "";
        if (EloChangeIcon != null) EloChangeIcon.enabled = false;
        if (EloChangeText != null) EloChangeText.text = "";
        if (PointsText != null) PointsText.text = "";
        if (PlaceholderText != null) PlaceholderText.text = text;
        return;
    }

    private void InitCompact(Match match, Sprite sprite, string name, int rank, string pointsText)
    {
        FlagIcon.sprite = sprite;
        NameText.text = name;
        Background.color = GetBackgroundColor(match, rank);

        if (match.IsDone) PointsText.text = pointsText;
        else PointsText.text = "";
    }

    private void InitFullPlayer(Match match, MatchParticipant_Player playerParticipant)
    {
        Sprite sprite = playerParticipant.Player.FlagSmall;
        string nameText = playerParticipant.Player.Name;
        int rank = match.GetPlayerRanking().IndexOf(playerParticipant);
        Color backgroundColor = GetBackgroundColor(match, rank);

        string pointsText = match.IsDone ? match.GetPlayerMatchScore(playerParticipant).ToString() : "";
        int currentElo = playerParticipant.Player.Elo[match.Discipline.Def];
        int currentLeaguePoints = (!match.IsDone && match.Tournament.League != null) ? playerParticipant.Player.CurrentLeaguePoints : 0;
        int eloBeforeMatch = playerParticipant.EloBeforeMatch;
        int eloAfterMatch = playerParticipant.EloAfterMatch;

        InitFull(match, sprite, nameText, pointsText, currentElo, currentLeaguePoints, eloBeforeMatch, eloAfterMatch, backgroundColor);
    }
    private void InitFullTeam(TeamMatch match, MatchParticipant_Team teamParticipant)
    {
        Sprite sprite = teamParticipant.Team.FlagSmall;
        string nameText = teamParticipant.Team.Name;
        int rank = match.GetTeamRanking().IndexOf(teamParticipant);
        Color backgroundColor = GetBackgroundColor(match, rank);

        string pointsText = match.IsDone ? match.GetTeamMatchScore(teamParticipant).ToString() : "";
        int currentElo = teamParticipant.Team.Elo[match.Discipline.Def];
        int currentLeaguePoints = 0;
        int eloBeforeMatch = teamParticipant.EloBeforeMatch;
        int eloAfterMatch = teamParticipant.EloAfterMatch;

        InitFull(match, sprite, nameText, pointsText, currentElo, currentLeaguePoints, eloBeforeMatch, eloAfterMatch, backgroundColor);
    }
    private void InitFull(Match match, Sprite sprite, string nameText, string pointsText, int currentElo, int currentLP, int eloBeforeMatch, int eloAfterMatch, Color backgroundColor)
    {
        FlagIcon.sprite = sprite;
        NameText.text = nameText;
        Background.color = backgroundColor;
        PlaceholderText.text = "";

        if (match.IsDone)
        {
            PointsText.text = pointsText;

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
