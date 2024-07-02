using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_MatchPlayer : MonoBehaviour
{
    public Image Background;
    public Image FlagIcon;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI PlusPointsText;
    public TextMeshProUGUI PointsText;

    private bool IsTeamMatch;

    public void Init(Player p, int points, bool isTeamMatch = false)
    {
        IsTeamMatch = isTeamMatch;

        FlagIcon.sprite = p.FlagSprite;
        NameText.text = p.Name;
        ScoreText.text = "";
        PlusPointsText.text = "";
        PointsText.text = IsTeamMatch ? "" : points.ToString();

        GetComponent<PlayerTooltipTarget>().Player = p;
    }

    public void InitEmpty()
    {
        FlagIcon.enabled = false;
        NameText.text = "";
        ScoreText.text = "";
        PlusPointsText.text = "";
        PointsText.text = "";
    }

    public void HideResult()
    {
        ScoreText.text = "";
        PlusPointsText.text = "";

        if (IsTeamMatch) PointsText.text = "";
    }

    public void DisplayResult(PlayerMatchRound round)
    {
        ScoreText.text = round.Score.ToString();
        ScoreText.color = round.Modifiers.Contains(Player.MISTAKE_MODIFIER) ? Color.red : Color.white;

        if(IsTeamMatch) PointsText.text = "+" + round.PointsGained.ToString();
        else PlusPointsText.text = "+" + round.PointsGained.ToString();
    }
}
