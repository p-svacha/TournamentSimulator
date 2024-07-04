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
        foreach (SkillDef skillDef in TournamentSimulator.SkillDefs)
        {
            TextMeshProUGUI skillText = Instantiate(TitleRow.SkillTextPrefab, TitleRow.SkillContainer.transform);
            skillText.text = skillDef.ThreeLetterDisplay;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(TitleRow.GetComponent<RectTransform>());

        // Player rows
        HelperFunctions.DestroyAllChildredImmediately(Container, skipElements: 1);
        foreach (MatchParticipant part in match.Ranking)
        {
            UI_MatchOverviewPlayer row = Instantiate(PlayerRowPrefab, Container.transform);
            row.Init(match, part);
        }
    }
}
