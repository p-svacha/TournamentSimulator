using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_MatchOverviewScreen : UI_Screen
{
    [Header("Elements")]
    public TextMeshProUGUI TitleText;
    public Button BackButton;
    public Button SimulateButton;
    public Button SimulateFastButton;

    public UI_MatchOverviewPlayer TitleRow;
    public GameObject Container;

    [Header("Prefabs")]
    public UI_MatchOverviewPlayer PlayerRowPrefab;

    public override void Init(UI_Base baseUI)
    {
        base.Init(baseUI);
    }

    public void DisplayMatch(Match m)
    {
        // Header
        TitleText.text = m.Name;

        BackButton.onClick.RemoveAllListeners();
        BackButton.onClick.AddListener(() => BaseUI.DisplayTournament(m.Tournament));

        if (m.CanSimulate())
        {
            SimulateButton.gameObject.SetActive(true);
            SimulateButton.onClick.AddListener(() => BaseUI.StartMatchSimulation(m, stepTime: 1.5f));
            SimulateFastButton.gameObject.SetActive(true);
            SimulateFastButton.onClick.AddListener(() => BaseUI.StartMatchSimulation(m, stepTime: 0.01f));
        }
        else
        {
            SimulateButton.gameObject.SetActive(false);
            SimulateFastButton.gameObject.SetActive(false);
        }

        // Title Row
        HelperFunctions.DestroyAllChildredImmediately(TitleRow.SkillContainer);
        foreach(SkillDef skillDef in TournamentSimulator.SkillDefs)
        {
            TextMeshProUGUI skillText = Instantiate(TitleRow.SkillTextPrefab, TitleRow.SkillContainer.transform);
            skillText.text = skillDef.ThreeLetterDisplay;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(TitleRow.GetComponent<RectTransform>());

        // Player rows
        HelperFunctions.DestroyAllChildredImmediately(Container, skipElements: 1);
        foreach(MatchParticipant part in m.Ranking)
        {
            UI_MatchOverviewPlayer row = Instantiate(PlayerRowPrefab, Container.transform);
            row.Init(m, part);
        }
    }
}
