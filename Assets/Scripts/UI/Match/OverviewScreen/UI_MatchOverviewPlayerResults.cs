using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_MatchOverviewPlayerResults : MonoBehaviour
{
    [Header("Element")]
    public UI_MatchOverviewPlayer TitleRow;
    public GameObject Container;

    [Header("Prefabs")]
    public UI_MatchOverviewPlayer PlayerRowPrefab;

    public void DisplayMatch(Match match)
    {
        // Title Row
        HelperFunctions.DestroyAllChildredImmediately(TitleRow.SkillContainer);

        List<SkillDef> skillColumns = new List<SkillDef>();
        if (match.Games.Count == 0) skillColumns.AddRange(match.Discipline.Skills); // Show discipline skills if match has not yet started
        else if (match.Games.Count == 1) skillColumns.AddRange(match.Games[0].Skills); // Show skills of only game
        else throw new System.NotImplementedException("Match display not implemented for matches with more than one game.");

        if (match.IsDone)
        {
            foreach (GameRound round in match.Games[0].Rounds)
            {
                TextMeshProUGUI skillText = Instantiate(TitleRow.SkillTextPrefab, TitleRow.SkillContainer.transform);
                skillText.text = round.Skill.Triplet;
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(TitleRow.GetComponent<RectTransform>());

        // Player rows
        HelperFunctions.DestroyAllChildredImmediately(Container);
        foreach (MatchParticipant_Player part in match.GetPlayerRanking())
        {
            UI_MatchOverviewPlayer row = Instantiate(PlayerRowPrefab, Container.transform);
            row.Init(match, part);
        }
    }
}
