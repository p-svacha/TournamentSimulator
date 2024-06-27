using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ScheduleElement : MonoBehaviour
{
    [Header("Elements")]
    public Image Background;
    public TextMeshProUGUI DateText;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI RoundText;
    public Button SimulateButton;

    public void Init(UI_Base baseUI, Tournament t, int absoluteDay)
    {
        int quarter = Database.ToRelativeQuarter(absoluteDay);
        int day = Database.ToRelativeDay(absoluteDay);

        if (t.IsDone || Database.Quarter > quarter || (Database.Quarter == quarter && Database.Day > day)) Background.color = ColorManager.Singleton.AdvanceColor; // Green
        else if (t.HasOpenMatchesToday()) Background.color = ColorManager.Singleton.OngoingColor;
        else Background.color = ColorManager.Singleton.DefaultColor;
        
        DateText.text = Database.GetQuarterName(quarter) + " " + day;
        NameText.text = t.Name;
        RoundText.text = t.GetMatchDayTitle(t.GetMatchDays().IndexOf(absoluteDay));
        SimulateButton.onClick.AddListener(() => baseUI.DisplayTournament(t));
    }
}
