using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_MatchPlayer : MonoBehaviour
{
    private Game Game;

    public Image Background;
    public Image FlagIcon;
    public TextMeshProUGUI RankText;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI PlusPointsText;
    public TextMeshProUGUI PointsText;

    public void Init(Game game, Player p, int points)
    {
        Game = game;

        FlagIcon.sprite = p.FlagBig;
        if (RankText != null) RankText.text = "";
        NameText.text = p.Name;
        ScoreText.text = "";
        PlusPointsText.text = "";
        PointsText.text = Game.IsTeamGame ? "" : points.ToString();

        GetComponent<PlayerTooltipTarget>().Init(game.Discipline.Def, p);
    }

    public void InitEmpty()
    {
        FlagIcon.enabled = false;
        if (RankText != null) RankText.text = "";
        NameText.text = "";
        ScoreText.text = "";
        PlusPointsText.text = "";
        PointsText.text = "";
    }

    public void HideResult()
    {
        ScoreText.text = "";
        PlusPointsText.text = "";

        if (Game.IsTeamGame) PointsText.text = "";
    }

    public void DisplayResult(PlayerGameRound round)
    {
        ScoreText.text = round.Score.ToString();
        ScoreText.color = round.Modifiers.Contains(Player.MISTAKE_MODIFIER) ? Color.red : Color.white;

        string sign = round.PointsGained >= 0 ? "+" : "";
        if (Game.IsTeamGame) PointsText.text = sign + round.PointsGained.ToString();
        else PlusPointsText.text = sign + round.PointsGained.ToString();
    }
}
