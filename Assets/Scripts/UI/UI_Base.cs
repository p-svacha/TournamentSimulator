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
    public UI_MatchScreen MatchScreen;

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
        MatchScreen.Init(this);

        Popup.gameObject.SetActive(false);

        ActiveScreen = DashboardScreen;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateTime(int season, int quarter, int day)
    {
        Header.UpdateTime(season, quarter, day);
    }

    public void UpdatePlayers(List<Player> players)
    {
        if(DashboardScreen.gameObject.activeSelf) DashboardScreen.UpdateRatingList(players);
        if(DashboardScreen.gameObject.activeSelf && Simulator.CurrentGrandLeague != null) DashboardScreen.UpdateGrandLeagueList(Simulator.CurrentGrandLeague);
        if(DashboardScreen.gameObject.activeSelf && Simulator.CurrentChallengeLeague != null) DashboardScreen.UpdateChallengeLeagueList(Simulator.CurrentChallengeLeague);
        if(DashboardScreen.gameObject.activeSelf && Simulator.CurrentOpenLeague != null) DashboardScreen.UpdateOpenLeagueList(Simulator.CurrentOpenLeague);
    }

    public void UpdateTournaments(List<Tournament> tournaments)
    {
        if (DashboardScreen.gameObject.activeSelf) DashboardScreen.UpdateSchedule(tournaments);
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

    public void DisplayMatchScreen(Match m)
    {
        DisplayScreen(MatchScreen);
        MatchScreen.DisplayMatch(m);
    }

    private void DisplayScreen(UI_Screen screen)
    {
        ActiveScreen.gameObject.SetActive(false);
        ActiveScreen = screen;
        ActiveScreen.gameObject.SetActive(true);
    }

    public void UpdateMedals(List<System.Tuple<Player, int, int, int>> medals)
    {
        DashboardScreen.UpdateMedalList(medals);
    }
}
