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
    public UI_MatchSimulationScreen MatchSimulationScreen_FreeForAll;
    public UI_1v1TeamMatchSimulationScreen MatchSimulationScreen_1v1TeamMatch;

    public UI_MatchOverviewScreen MatchOverviewScreen;
    public UI_1v1TeamMatchOverviewScreen Team1v1MatchOverviewScreen;

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
        MatchSimulationScreen_FreeForAll.Init(this);
        MatchSimulationScreen_1v1TeamMatch.Init(this);
        MatchOverviewScreen.Init(this);
        Team1v1MatchOverviewScreen.Init(this);

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

    public void DisplayMatchOverviewScreen(Match match)
    {
        if (match.Type == MatchType.FreeForAll)
        {
            DisplayScreen(MatchOverviewScreen);
            MatchOverviewScreen.DisplayMatch(match);
        }
        else if (match.Type == MatchType.TeamMatch_1v1)
        {
            DisplayScreen(Team1v1MatchOverviewScreen);
            Team1v1MatchOverviewScreen.DisplayMatch((TeamMatch)match);
        }
    }

    public void StartMatchSimulation(Match match, float stepTime)
    {
        if (match.Type == MatchType.FreeForAll)
        {
            DisplayScreen(MatchSimulationScreen_FreeForAll);
            MatchSimulationScreen_FreeForAll.DisplayAndSimulateMatch(match, stepTime);
        }
        else if(match.Type == MatchType.TeamMatch_1v1)
        {
            DisplayScreen(MatchSimulationScreen_1v1TeamMatch);
            MatchSimulationScreen_1v1TeamMatch.DisplayAndSimulateMatch((TeamMatch)match, stepTime);
        }
    }



    private void DisplayScreen(UI_Screen screen)
    {
        // Hide tooltip
        UI_PlayerTooltip.Singleton.Hide();

        // Switch screen
        ActiveScreen.gameObject.SetActive(false);
        ActiveScreen = screen;
        ActiveScreen.gameObject.SetActive(true);
    }
}
