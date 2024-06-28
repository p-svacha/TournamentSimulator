using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Tournament : UI_Screen
{
    [Header("Elements")]
    public GameObject Container;
    public Button BackButton;

    private List<UI_TMatch> Matches;

    public override void Init(UI_Base baseUI)
    {
        base.Init(baseUI);

        BackButton.onClick.AddListener(BaseUI.DisplayDashboard);
    }

    public void DisplayTournament(Tournament t)
    {
        HelperFunctions.DestroyAllChildredImmediately(Container);
        Matches = t.DisplayTournament(BaseUI, Container);
    }
}
