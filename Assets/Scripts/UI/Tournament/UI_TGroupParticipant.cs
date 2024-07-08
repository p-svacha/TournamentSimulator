using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_TGroupParticipant : MonoBehaviour
{
    [Header("Elements")]
    public Image RankBackground;
    public TextMeshProUGUI RankText;
    public Image Flag;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI NumGamesText;
    public TextMeshProUGUI TotalPointsText;
    public TextMeshProUGUI GroupPointsText;

    public void Init(TournamentGroupParticipant p)
    {
        if(p.Team != null)
        {
            if (p.Group.IsDone && p.Rank < p.Group.NumAdvancements) RankBackground.color = new Color(RankBackground.color.r, RankBackground.color.g, RankBackground.color.b, 1f);
            RankText.text = p.Rank.ToString() + ".";
            Flag.sprite = p.Team.FlagSmall;
            NameText.text = p.Team.Name;
            NumGamesText.text = p.NumMatches.ToString();
            TotalPointsText.text = p.TotalMatchPointsGained + ":" + p.TotalMatchPointsLost;
            GroupPointsText.text = p.GroupPoints.ToString();
        }
    }
}
