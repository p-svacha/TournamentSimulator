using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Base : MonoBehaviour
{
    public static UI_Base Instance;

    [HideInInspector]
    public TournamentSimulator Simulator;

    public UI_Header Header;

    [Header("Screens")]
    public UI_Dashboard DashboardScreen;
    public UI_Tournament TournamentScreen;
    public UI_SoloFfaGameSimulationScreen GameSimulationScreen_SoloFreeForAll;
    public UI_1v1TeamGameSimulationScreen GameSimulationScreen_Team1v1;

    public UI_MatchOverviewScreen MatchOverviewScreen;
    public UI_1v1TeamMatchOverviewScreen Team1v1MatchOverviewScreen;

    private UI_Screen ActiveScreen;

    [Header("Popup")]
    public UI_Popup Popup;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    public void Init(TournamentSimulator sim)
    {
        Simulator = sim;
        Popup.BaseUI = this;

        Header.Init(this);
        DashboardScreen.Init(this);
        TournamentScreen.Init(this);
        GameSimulationScreen_SoloFreeForAll.Init(this);
        GameSimulationScreen_Team1v1.Init(this);
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
        if (match.IsTeamMatch)
        {
            DisplayScreen(Team1v1MatchOverviewScreen);
            Team1v1MatchOverviewScreen.DisplayMatch((TeamMatch)match);
        }
        else
        {
            DisplayScreen(MatchOverviewScreen);
            MatchOverviewScreen.DisplayMatch(match);
        }
    }

    public void StartGameSimulation(Game game, float stepTime)
    {
        if (game.IsTeamGame)
        {
            DisplayScreen(GameSimulationScreen_Team1v1);
            GameSimulationScreen_Team1v1.DisplayAndSimulateMatch((TeamGame)game, stepTime);
        }
        else
        {
            DisplayScreen(GameSimulationScreen_SoloFreeForAll);
            GameSimulationScreen_SoloFreeForAll.DisplayAndSimulateMatch((SoloGame)game, stepTime);
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
