using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_1v1TeamMatchSimulationScreen : UI_Screen
{
    private TeamMatch Match;

    [Header("Elements")]
    public UI_1v1ResultDisplay MatchHeader;

    public TextMeshProUGUI ProgressTitleText;
    public TextMeshProUGUI AttributeTitleText;

    public GameObject PlayerContainer;

    [Header("Prefabs")]
    public UI_MatchPlayer MatchPlayerPrefab;
    private Dictionary<Player, UI_MatchPlayer> PlayerRows;
    private UI_MatchPlayer SpacingRow;

    // Simulation
    private bool IsSimulating;
    private int CurrentSkillIndex;
    private SkillDef CurrentSkill;
    private int SimPlayer;
    private float TimeElapsed;
    private float StepTime;

    private MatchRound CurrentMatchRound;

    public void DisplayAndSimulateMatch(TeamMatch m, float stepTime)
    {
        Match = m;
        StartSimulation(stepTime);
    }

    private void StartSimulation(float stepTime)
    {
        StepTime = stepTime;
        IsSimulating = true;
        CurrentSkillIndex = -1;
        SimPlayer = Match.NumPlayers + 1;
        TimeElapsed = 0;

        Match.StartMatch();

        // Init UI
        ProgressTitleText.text = "";
        AttributeTitleText.text = "";

        // Teams
        MatchHeader.DisplayMatch(Match.TeamParticipants[0], Match.TeamParticipants[1]);

        // Players
        HelperFunctions.DestroyAllChildredImmediately(PlayerContainer, skipElements: 1);

        PlayerRows = new Dictionary<Player, UI_MatchPlayer>();
        foreach (MatchParticipant p in Match.Ranking)
        {
            UI_MatchPlayer row = Instantiate(MatchPlayerPrefab, PlayerContainer.transform);
            row.Init(p.Player, p.TotalPoints, isTeamMatch: true);
            row.Background.color = new Color(p.Team.Color1.r, p.Team.Color1.g, p.Team.Color1.b, 0.2f);
            PlayerRows.Add(p.Player, row);
        }

        // Add an empty player row that acts as spacing between players that have finished the current round and those that haven't
        SpacingRow = Instantiate(MatchPlayerPrefab, PlayerContainer.transform);
        SpacingRow.InitEmpty();
    }

    private void EndSimulation()
    {
        IsSimulating = false;
        Match.SetDone();
        BaseUI.Simulator.Save();
        BaseUI.DisplayMatchOverviewScreen(Match);
    }

    void Update()
    {
        TimeElapsed += Time.deltaTime;
        if (IsSimulating && TimeElapsed >= StepTime)
        {
            TimeElapsed = 0;

            if (SimPlayer == Match.NumPlayers)
            {
                EndRound();
                SimPlayer++;
            }
            else if (SimPlayer == Match.NumPlayers + 1)
            {
                if (CurrentSkillIndex == TournamentSimulator.SkillDefs.Count - 1) EndSimulation();
                else GoToNextSkill();
            }
            else
            {
                DisplayNextPlayerResult();
            }
        }
    }

    private void DisplayNextPlayerResult()
    {
        Player p = CurrentMatchRound.PlayerRanking[CurrentMatchRound.PlayerRanking.Count - SimPlayer - 1];
        PlayerRows[p].DisplayResult(CurrentMatchRound.GetPlayerResult(p));

        PlayerRows[p].transform.SetSiblingIndex(1);

        SimPlayer++;
    }

    private void EndRound()
    {
        // Save match round
        Match.ApplyMatchRound(CurrentMatchRound);

        // Clear row texts
        foreach (MatchParticipant p in Match.Participants)
        {
            PlayerRows[p.Player].HideResult();
        }

        // Update total scores
        foreach (MatchParticipant participant in Match.Participants)
        {
            PlayerRows[participant.Player].PlusPointsText.text = participant.TotalPoints.ToString();
        }

        // Update team scores
        MatchHeader.DisplayMatch(Match.TeamParticipants[0], Match.TeamParticipants[1]);
    }

    private void GoToNextSkill()
    {
        // Change attribute
        CurrentSkillIndex++;
        CurrentSkill = TournamentSimulator.SkillDefs[CurrentSkillIndex];
        ProgressTitleText.text = (CurrentSkillIndex + 1) + "/" + TournamentSimulator.SkillDefs.Count;
        AttributeTitleText.text = TournamentSimulator.SkillDefs[CurrentSkillIndex].DisplayName;
        SimPlayer = 0;

        // Execute match round
        CurrentMatchRound = Match.CalculateRoundResult(CurrentSkill);

        // Move spacing row to top and sort by player ranking
        List<Player> playerRanking = Match.PlayerRanking;
        for (int i = 0; i < playerRanking.Count; i++) PlayerRows[playerRanking[i]].transform.SetSiblingIndex(i + 1);

        SpacingRow.transform.SetSiblingIndex(1);
    }
}
