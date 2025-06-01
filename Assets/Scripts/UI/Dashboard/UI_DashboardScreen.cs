using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_DashboardScreen : UI_Screen
{
    [Header("Elements")]
    public TMP_Dropdown DisciplineDropdown;
    public TMP_Dropdown SeasonDropdown;
    public TMP_Dropdown DashboardDropdown;
    public List<UI_Dashboard> Dashboards;

    // State
    public int SelectedDisciplineIndex { get; set; }
    public int SelectedSeasonIndex { get; set; }
    public int SelectedDashboardIndex { get; set; }

    public override void Init(UI_Base baseUI)
    {
        base.Init(baseUI);

        // Discipline dropdown
        DisciplineDropdown.options = DefDatabase<DisciplineDef>.AllDefs.Select(x => new TMP_Dropdown.OptionData(x.Label)).ToList();
        DisciplineDropdown.onValueChanged.AddListener(SelectDiscipline);

        // Dashboard dropdown
        foreach (UI_Dashboard dashboard in Dashboards) dashboard.Init();
        List<TMP_Dropdown.OptionData> dashboardOptions = Dashboards.Select(d => new TMP_Dropdown.OptionData(d.Label)).ToList();
        DashboardDropdown.options = dashboardOptions;
        DashboardDropdown.onValueChanged.AddListener(SelectDashboard);

        // Season dropdown
        List<TMP_Dropdown.OptionData> seasonOptions = new List<TMP_Dropdown.OptionData>();
        for (int i = 0; i < Database.Season; i++)
        {
            seasonOptions.Add(new TMP_Dropdown.OptionData($"Season {(i + 1)}"));
        }
        SeasonDropdown.options = seasonOptions;
        SeasonDropdown.onValueChanged.AddListener(SelectSeason);
        SelectSeason(Database.Season);
    }

    private void SelectDiscipline(int disciplineIndex)
    {
        SelectedDisciplineIndex = disciplineIndex;
        Refresh();
    }
    private void SelectSeason(int seasonIndex)
    {
        SelectedSeasonIndex = seasonIndex;
        Refresh();
    }
    private void SelectDashboard(int dashboardIndex)
    {
        SelectedDashboardIndex = dashboardIndex;
        Refresh();
    }

    public void Refresh()
    {
        if (SelectedDisciplineIndex != DisciplineDropdown.value) DisciplineDropdown.value = SelectedDisciplineIndex;
        if (SelectedSeasonIndex != SeasonDropdown.value) SeasonDropdown.value = SelectedSeasonIndex;
        if (SelectedDashboardIndex != DashboardDropdown.value) DashboardDropdown.value = SelectedDashboardIndex;
        for(int i = 0; i < Dashboards.Count; i++)
        {
            if (i == SelectedDashboardIndex)
            {
                Dashboards[i].gameObject.SetActive(true);
                Dashboards[i].Refresh(DefDatabase<DisciplineDef>.AllDefs[SelectedDisciplineIndex], SelectedSeasonIndex + 1);
            }
            else Dashboards[i].gameObject.SetActive(false);
        }
    }
}
