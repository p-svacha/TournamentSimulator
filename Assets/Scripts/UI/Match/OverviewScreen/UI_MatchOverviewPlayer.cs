using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class UI_MatchOverviewPlayer : MonoBehaviour
{
    [Header("Elements")]
    public Image FlagIcon;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI ScoreText;
    public GameObject SkillContainer;

    [Header("Prefabs")]
    public TextMeshProUGUI SkillTextPrefab;

    public void Init(Match m, MatchParticipant_Player p)
    {
        GetComponent<PlayerTooltipTarget>().Init(m.Discipline.Def, p.Player);

        FlagIcon.sprite = p.Player.FlagBig;
        NameText.text = p.Player.Name;
        ScoreText.text = "";

        if (m.IsDone)
        {
            ScoreText.text = p.MatchScore.ToString();

            if (m.Games.Count == 1)
            {
                Game game = m.Games[0];

                foreach (SkillDef skillDef in game.Skills)
                {
                    GameRound round = m.Games[0].Rounds.First(x => x.Skill == skillDef);
                    PlayerGameRound pRound = round.PlayerResults.First(x => x.Player == p.Player);

                    int score = pRound.Score;

                    TextMeshProUGUI skillText = Instantiate(SkillTextPrefab, SkillContainer.transform);
                    skillText.text = score.ToString();
                    if (pRound.Modifiers != null && pRound.Modifiers.Contains(Player.MISTAKE_MODIFIER)) skillText.color = Color.red;
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            }

            else throw new System.Exception("Not implemented yet for games with multiple matches.");
        }
    }
}
