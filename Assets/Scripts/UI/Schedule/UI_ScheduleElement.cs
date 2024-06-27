using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ScheduleElement : MonoBehaviour
{
    [Header("Elements")]
    public Image Background;
    public Text DateText;
    public Text NameText;
    public Button SimulateButton;

    public void Init(UI_Base baseUI, Tournament t)
    {
        if (t.IsDone) Background.color = ColorManager.Singleton.AdvanceColor;
        else if (t.Day == Database.Day && t.Quarter == Database.Quarter) Background.color = ColorManager.Singleton.OngoingColor;
        else Background.color = ColorManager.Singleton.DefaultColor;
        
        DateText.text = Database.GetQuarterName(t.Quarter) + " " + t.Day;
        NameText.text = t.Name;
        SimulateButton.onClick.AddListener(() => baseUI.DisplayTournament(t));
    }
}
