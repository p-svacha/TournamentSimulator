using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_1v1TeamMatchOverviewScreen : UI_Screen
{
    [Header("Elements")]
    public UI_MatchOverviewHeader OverviewHeader;
    public UI_1v1MatchHeader Result1v1Header;
    public UI_MatchOverviewPlayerResults PlayerResultList;

    public override void Init(UI_Base baseUI)
    {
        base.Init(baseUI);
    }

    public void DisplayMatch(TeamMatch m)
    {
        OverviewHeader.DisplayMatch(BaseUI, m);
        Result1v1Header.DisplayMatch(m.TeamParticipants[0], m.TeamParticipants[1]);
        PlayerResultList.DisplayMatch(m);
    }
}
