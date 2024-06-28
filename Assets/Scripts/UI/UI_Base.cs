using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Base : MonoBehaviour
{
    [HideInInspector]
    public TournamentSimulator Simulator;

    public UI_Header Header;

    [Header("Screens")]
    public UI_Dashboard DashboardScreen;
    public UI_Tournament TournamentScreen;
    public UI_MatchSimulationScreen MatchSimulationScreen;
    public UI_MatchOverviewScreen MatchOverviewScreen;

    private UI_Screen ActiveScreen;

    [Header("Popup")]
    public UI_Popup Popup;

    // Start is called before the first frame update
    public void Init(TournamentSimulator sim)
    {
        Simulator = sim;
        Popup.BaseUI = this;

        Header.Init(this);
        DashboardScreen.Init(this);
        TournamentScreen.Init(this);
        MatchSimulationScreen.Init(this);
        MatchOverviewScreen.Init(this);

        Popup.gameObject.SetActive(false);

        ActiveScreen = DashboardScreen;
    }

    public void UpdateTime()
    {
        Header.UpdateTime(Database.Season, Database.Quarter, Database.Day);
    }

    public void DisplayTournament(Tournament t)
    {
        DisplayScreen(TournamentScreen);
        TournamentScreen.DisplayTournament(t);
    }

    public void DisplayDashboard()
    {
        DisplayScreen(DashboardScreen);
        Simulator.UpdateUI();
    }

    public void DisplayMatchOverviewScreen(Match m)
    {
        DisplayScreen(MatchOverviewScreen);
        MatchOverviewScreen.DisplayMatch(m);
    }

    public void StartMatchSimulation(Match m, float stepTime)
    {
        DisplayScreen(MatchSimulationScreen);
        MatchSimulationScreen.DisplayAndSimulateMatch(m, stepTime);
    }

    private void DisplayScreen(UI_Screen screen)
    {
        ActiveScreen.gameObject.SetActive(false);
        ActiveScreen = screen;
        ActiveScreen.gameObject.SetActive(true);
    }
}
