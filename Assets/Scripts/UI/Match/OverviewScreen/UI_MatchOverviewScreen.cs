using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_MatchOverviewScreen : UI_Screen
{
    [Header("Elements")]
    public UI_MatchOverviewHeader Header;
    public UI_MatchOverviewPlayerResults PlayerResultList;

    public override void Init(UI_Base baseUI)
    {
        base.Init(baseUI);
    }

    public void DisplayMatch(Match m)
    {
        Header.DisplayMatch(BaseUI, m);
        PlayerResultList.DisplayMatch(m);
    }
}
