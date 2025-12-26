using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class UI_MatchOverviewPlayer : MonoBehaviour
{
    [Header("Elements")]
    public Image FlagIcon;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI TiebreakerText;
    public TextMeshProUGUI EloChangeText;
    public GameObject SkillContainer;

    [Header("Prefabs")]
    public TextMeshProUGUI SkillTextPrefab;

    public void Init(Match m, MatchParticipant_Player p)
    {
        GetComponent<PlayerTooltipTarget>().Init(m.Discipline.Def, p.Player);

        FlagIcon.sprite = p.Player.FlagBig;
        NameText.text = p.Player.Name;
        ScoreText.text = "";
        TiebreakerText.text = "";
        EloChangeText.text = "";

        if (m.IsDone)
        {
            ScoreText.text = m.GetPlayerMatchScore(p).ToString();
            TiebreakerText.text = m.GetPlayerMatchTiebreakerScore(p).ToString();
            EloChangeText.text = (p.EloChange >= 0 ? "+" : "") + p.EloChange;
            if (p.EloChange < 0) EloChangeText.color = ColorManager.Singleton.RedTextColor;
            else if (p.EloChange > 0) EloChangeText.color = ColorManager.Singleton.GreenTextColor;

            if (m.Games.Count == 1)
            {
                Game game = m.Games[0];

                foreach (GameRound round in game.Rounds)
                {
                    TextMeshProUGUI skillText = Instantiate(SkillTextPrefab, SkillContainer.transform);

                    if (round == null)
                    {
                        skillText.text = "";
                        continue;
                    }

                    PlayerGameRound pRound = round.PlayerResults.FirstOrDefault(x => x.Player == p.Player);
                    if (pRound == null)
                    {
                        skillText.text = "-";
                    }
                    else
                    {
                        int score = pRound.Score;
                        skillText.text = score.ToString();
                        if (pRound.Modifiers != null && pRound.Modifiers.Contains(Player.MISTAKE_MODIFIER)) skillText.color = Color.red;
                    }
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            }

            else throw new System.Exception("Not implemented yet for games with multiple matches.");
        }
    }
}
