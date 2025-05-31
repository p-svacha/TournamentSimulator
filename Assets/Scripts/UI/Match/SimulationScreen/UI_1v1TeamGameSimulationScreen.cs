using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class UI_1v1TeamGameSimulationScreen : UI_Screen
{
    private TeamGame Game;
    private TeamMatch Match => Game.Match;

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

    private TeamGameRound CurrentGameRound;

    public void DisplayAndSimulateMatch(TeamGame game, float stepTime)
    {
        Game = game;
        StartSimulation(stepTime);
    }

    private void StartSimulation(float stepTime)
    {
        StepTime = stepTime;
        IsSimulating = true;
        CurrentSkillIndex = -1;
        SimPlayer = Match.NumPlayers + 1;
        TimeElapsed = 0;

        Game.StartGame();

        // Init UI
        ProgressTitleText.text = "";
        AttributeTitleText.text = "";

        // Teams
        MatchHeader.DisplayMatch(Match.TeamParticipants[0], Match.TeamParticipants[1]);

        // Team colors
        Dictionary<Team, Color> teamColors = new Dictionary<Team, Color>();
        foreach(MatchParticipant_Team team in Match.TeamParticipants)
        {
            Color c = team.Team.Color1;
            if (teamColors.Values.Any(x => ColorManager.Singleton.AreColorsSimilar(x, c))) c = team.Team.Color2;
            teamColors.Add(team.Team, c);
        }

        // Players
        HelperFunctions.DestroyAllChildredImmediately(PlayerContainer, skipElements: 1);

        PlayerRows = new Dictionary<Player, UI_MatchPlayer>();
        foreach (MatchParticipant_Player p in Match.PlayerParticipantRanking)
        {
            UI_MatchPlayer row = Instantiate(MatchPlayerPrefab, PlayerContainer.transform);
            row.Init(p.Player, p.MatchScore, isTeamMatch: true);
            Color teamColor = teamColors[p.Team];
            row.Background.color = new Color(teamColor.r, teamColor.g, teamColor.b, 0.2f);
            PlayerRows.Add(p.Player, row);
        }

        // Add an empty player row that acts as spacing between players that have finished the current round and those that haven't
        SpacingRow = Instantiate(MatchPlayerPrefab, PlayerContainer.transform);
        SpacingRow.InitEmpty();
    }

    private void EndSimulation()
    {
        IsSimulating = false;
        Game.SetDone();
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
                if (CurrentSkillIndex == DefDatabase<SkillDef>.AllDefs.Count - 1) EndSimulation();
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
        Player p = CurrentGameRound.PlayerRanking[CurrentGameRound.PlayerRanking.Count - SimPlayer - 1];
        PlayerRows[p].DisplayResult(CurrentGameRound.GetPlayerResult(p));

        PlayerRows[p].transform.SetSiblingIndex(1);

        SimPlayer++;
    }

    private void EndRound()
    {
        // Save game round
        Game.ApplyGameRound(CurrentGameRound);

        // Clear row texts
        foreach (MatchParticipant_Player p in Match.PlayerParticipants)
        {
            PlayerRows[p.Player].HideResult();
        }

        // Update total scores
        foreach (MatchParticipant_Player participant in Match.PlayerParticipants)
        {
            PlayerRows[participant.Player].PlusPointsText.text = participant.MatchScore.ToString();
        }

        // Update team scores
        MatchHeader.DisplayMatch(Match.TeamParticipants[0], Match.TeamParticipants[1]);
    }

    private void GoToNextSkill()
    {
        // Change attribute
        CurrentSkillIndex++;
        CurrentSkill = DefDatabase<SkillDef>.AllDefs[CurrentSkillIndex];
        ProgressTitleText.text = (CurrentSkillIndex + 1) + "/" + DefDatabase<SkillDef>.AllDefs.Count;
        AttributeTitleText.text = DefDatabase<SkillDef>.AllDefs[CurrentSkillIndex].Label;
        SimPlayer = 0;

        // Execute game round
        CurrentGameRound = Game.CreateGameRound(CurrentSkill);

        // Move spacing row to top and sort by player ranking
        List<Player> playerRanking = Match.PlayerRanking;
        for (int i = 0; i < playerRanking.Count; i++) PlayerRows[playerRanking[i]].transform.SetSiblingIndex(i + 1);

        SpacingRow.transform.SetSiblingIndex(1);
    }
}
