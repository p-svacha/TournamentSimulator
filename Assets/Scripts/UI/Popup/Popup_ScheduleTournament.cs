using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Popup_ScheduleTournament : PopupContent
{
    public Dropdown TypeDropdown;
    public Dropdown QuarterDropdown;
    public Dropdown DayDropdown;

    public override string PopupTitle { get => "Schedule Tournament"; }

    public override void OnInitShow() { }

    public override void OnOkClick(TournamentSimulator simulator)
    {
        LeagueType type = TypeDropdown.value == 0 ? LeagueType.GrandLeague : LeagueType.ChallengeLeague;
        simulator.ScheduleTournament(type, QuarterDropdown.value + 1, DayDropdown.value + 1);
    }
}
