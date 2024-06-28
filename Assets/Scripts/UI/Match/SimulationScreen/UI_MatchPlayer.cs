using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_MatchPlayer : MonoBehaviour
{
    public Image FlagIcon;
    public Text NameText;
    public Text ScoreText;
    public Text PlusPointsText;
    public Text PointsText;

    public void Init(Player p, int points)
    {
        FlagIcon.sprite = p.FlagSprite;
        NameText.text = p.Name;
        ScoreText.text = "";
        PlusPointsText.text = "";
        PointsText.text = points.ToString();

        GetComponent<PlayerTooltipTarget>().Player = p;
    }

    public void HideResult()
    {
        ScoreText.text = "";
        PlusPointsText.text = "";
    }

    public void DisplayResult(PlayerMatchRound round)
    {
        ScoreText.text = round.Score.ToString();
        ScoreText.color = round.Modifiers.Contains(Player.MISTAKE_MODIFIER) ? Color.red : Color.white;
        PlusPointsText.text = "+" + round.PointsGained.ToString();
    }
}
