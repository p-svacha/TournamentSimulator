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

    public void Init(Match m, MatchParticipant p)
    {
        GetComponent<PlayerTooltipTarget>().Player = p.Player;

        FlagIcon.sprite = p.Player.FlagSprite;
        NameText.text = p.Player.Name;
        ScoreText.text = "";

        if (m.IsDone)
        {
            ScoreText.text = p.TotalPoints.ToString();

            if (m.Rounds.Count > 1)
            {
                foreach (SkillDef skillDef in TournamentSimulator.SkillDefs)
                {
                    MatchRound round = m.Rounds.First(x => x.SkillId == skillDef.Id);
                    PlayerMatchRound pRound = round.PlayerResults.First(x => x.Player == p.Player);

                    int score = pRound.Score;

                    TextMeshProUGUI skillText = Instantiate(SkillTextPrefab, SkillContainer.transform);
                    skillText.text = score.ToString();
                    if (pRound.Modifiers != null && pRound.Modifiers.Contains(Player.MISTAKE_MODIFIER)) skillText.color = Color.red;
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            }
        }
    }
}
