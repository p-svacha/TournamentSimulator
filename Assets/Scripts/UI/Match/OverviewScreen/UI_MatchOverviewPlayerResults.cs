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
        foreach (SkillDef skillDef in DefDatabase<SkillDef>.AllDefs)
        {
            TextMeshProUGUI skillText = Instantiate(TitleRow.SkillTextPrefab, TitleRow.SkillContainer.transform);
            skillText.text = skillDef.Triplet;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(TitleRow.GetComponent<RectTransform>());

        // Player rows
        HelperFunctions.DestroyAllChildredImmediately(Container, skipElements: 1);
        foreach (MatchParticipant_Player part in match.PlayerParticipantRanking)
        {
            UI_MatchOverviewPlayer row = Instantiate(PlayerRowPrefab, Container.transform);
            row.Init(match, part);
        }
    }
}
