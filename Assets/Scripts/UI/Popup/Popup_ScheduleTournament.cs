using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Popup_ScheduleTournament : PopupContent
{
    public Dropdown TypeDropdown;
    public Dropdown QuarterDropdown;
    public Dropdown DayDropdown;

    public override string PopupTitle => "Schedule Tournament";

    private void Start()
    {
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        foreach(TournamentType x in System.Enum.GetValues(typeof(TournamentType))) 
        {
            if(x != TournamentType.None) options.Add(new Dropdown.OptionData(x.ToString()));
        }
        TypeDropdown.options = options;
    }

    public override void OnInitShow() { }

    public override void OnOkClick(TournamentSimulator simulator)
    {
        TournamentType type = (TournamentType)TypeDropdown.value;
        simulator.ScheduleTournament(DisciplineDefOf.Football, type, QuarterDropdown.value + 1, DayDropdown.value + 1);
        simulator.UpdateUI();
    }
}
