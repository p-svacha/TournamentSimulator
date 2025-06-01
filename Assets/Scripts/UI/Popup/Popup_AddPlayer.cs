using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Popup_AddPlayer : PopupContent
{
    public Dropdown LocationDropdown;
    public InputField AmountInput;
    public InputField RatingInput;

    public override string PopupTitle { get => "Add Player"; }

    private Dictionary<int, string> LocationValues = new Dictionary<int, string>()
    {
        {0, "" },
        {1, "North America" },
        {2, "South America" },
        {3, "Europe" },
        {4, "Africa" },
        {5, "Middle East" },
        {6, "Asia" },
        {7, "Oceania" },
        {8, "Laurentia" },
        {9, "Caribbean" },
        {10, "Central America" },
        {11, "Andean" },
        {12, "Southern Cone" },
        {13, "Amazon" },
        {14, "Sahara" },
        {15, "West Africa" },
        {16, "Horn of Africa" },
        {17, "Equatorial Africa" },
        {18, "Southern Africa" },
        {19, "Eastern Europe" },
        {20, "Western Europe" },
        {21, "Northern Europe" },
        {22, "Caucasus" },
        {23, "Arabia" },
        {24, "Central Asia" },
        {25, "Northern Asia" },
        {26, "Subcontinent" },
        {27, "Southeast Asia" },
        {28, "Archipelago" },
        {29, "Australasia" },
        {30, "Pacific Islands" },
    };

    public override void OnInitShow()
    {
        AmountInput.text = "1";
        RatingInput.text = TournamentSimulator.DEFAULT_RATING.ToString();
    }

    public override void OnOkClick(TournamentSimulator simulator)
    {
        string region = "";
        string continent = "";

        if (LocationDropdown.value > 7) region = LocationValues[LocationDropdown.value];
        else if (LocationDropdown.value > 0) continent = LocationValues[LocationDropdown.value];

        for(int i = 0; i < int.Parse(AmountInput.text); i++) simulator.AddRandomPlayer(region, continent);

    }
}
