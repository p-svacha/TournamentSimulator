using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_GeneralDashboard : UI_Dashboard
{
    [Header("Elements")]
    public UI_Leaderboard Leaderboard;

    public override string Label => "General Dashboard";

    public override void Init()
    {
        Leaderboard.Init();
    }

    public override void Refresh(DisciplineDef discipline, int season)
    {
        Leaderboard.Refresh(discipline);
    }
}
