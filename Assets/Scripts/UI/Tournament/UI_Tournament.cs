using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Tournament : UI_Screen
{
    [Header("Elements")]
    public GameObject Container;
    public Button BackButton;

    [Header("Prefabs")]
    public UI_Group GroupPrefab;

    private List<UI_Group> Matches;

    public override void Init(UI_Base baseUI)
    {
        base.Init(baseUI);

        BackButton.onClick.AddListener(BaseUI.DisplayDashboard);
    }

    public void DisplayTournament(Tournament t)
    {
        HelperFunctions.DestroyAllChildredImmediately(Container);
        Matches = t.DisplayTournament(BaseUI, Container, GroupPrefab);
    }
}
