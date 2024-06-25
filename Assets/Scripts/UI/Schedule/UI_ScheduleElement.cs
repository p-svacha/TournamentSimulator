using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ScheduleElement : MonoBehaviour
{
    private UI_Base BaseUI;

    [Header("Elements")]
    public Image Background;
    public Text DateText;
    public Text NameText;
    public Button SimulateButton;

    public void Init(UI_Base baseUI, Tournament t)
    {
        BaseUI = baseUI;

        if (t.IsDone) Background.color = ColorManager.Singleton.AdvanceColor;
        else if (t.Day == BaseUI.Simulator.Day && t.Quarter == BaseUI.Simulator.Quarter) Background.color = ColorManager.Singleton.OngoingColor;
        else Background.color = ColorManager.Singleton.DefaultColor;
        
        DateText.text = TournamentSimulator.GetQuarterName(t.Quarter) + " " + t.Day;
        NameText.text = t.Name;
        SimulateButton.onClick.AddListener(() => BaseUI.DisplayTournament(t));
    }
}
