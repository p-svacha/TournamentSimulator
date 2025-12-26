using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SoloFfaGameSimulationScreen : UI_Screen
{
    private SoloGame Game;
    private SoloMatch Match => Game.Match;

    [Header("Elements")]
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI SubtitleText;
    public GameObject PlayerContainer_LowPlayerCount;
    public GameObject PlayerContainer_HighPlayerCount_Left;
    public GameObject PlayerContainer_HighPlayerCount_Right;

    [Header("Prefabs")]
    public UI_MatchPlayer MatchPlayerPrefab;
    public UI_MatchPlayer MatchPlayerPrefab_BigDisplay;
    public GameObject AdvancementSeparatorPrefab;
    private Dictionary<Player, UI_MatchPlayer> PlayerRows;
    private List<GameObject> AdvancementSeparators;

    // Simulation
    private bool IsSimulating;
    private int CurrentSkillIndex;
    private SkillDef CurrentSkill;
    private int SimPlayer;
    private float TimeElapsed;
    private float StepTime;

    private SoloGameRound CurrentGameRound;
    private List<int> AdvancementLimits;

    private bool IsUsingBigDisplay;
    private const int NUM_ROWS_IN_HIGH_CONT = 16;
    private const int PLAYER_COUNT_SEPARATOR = 10; // For matches with more players than this, the preview for high player counts is used.

    public void DisplayAndSimulateGame(SoloGame game, float stepTime)
    {
        Game = game;
        AdvancementLimits = Game.Match.AdvancementLimits;

        TitleText.text = game.Label;
        SubtitleText.text = "";
        PlayerRows = new Dictionary<Player, UI_MatchPlayer>();
        AdvancementSeparators = new List<GameObject>();

        IsUsingBigDisplay = game.Match.NumPlayerParticipants > PLAYER_COUNT_SEPARATOR;

        if (IsUsingBigDisplay)
        {
            PlayerContainer_LowPlayerCount.SetActive(false);
            PlayerContainer_HighPlayerCount_Left.SetActive(true);
            PlayerContainer_HighPlayerCount_Right.SetActive(true);

            HelperFunctions.DestroyAllChildredImmediately(PlayerContainer_HighPlayerCount_Left);
            HelperFunctions.DestroyAllChildredImmediately(PlayerContainer_HighPlayerCount_Right);

            int index = 0;
            foreach (MatchParticipant_Player p in Match.PlayerParticipants)
            {
                GameObject container = index < NUM_ROWS_IN_HIGH_CONT ? PlayerContainer_HighPlayerCount_Left : PlayerContainer_HighPlayerCount_Right;
                UI_MatchPlayer row = Instantiate(MatchPlayerPrefab_BigDisplay, container.transform);
                row.Init(game, p.Player, game.GetPlayerPoints(p));
                PlayerRows.Add(p.Player, row);

                if (AdvancementLimits.Contains(index))
                {
                    GameObject sep = Instantiate(AdvancementSeparatorPrefab, container.transform);
                    AdvancementSeparators.Add(sep);
                }

                index++;
            }
        }
        else
        {
            PlayerContainer_LowPlayerCount.SetActive(true);
            PlayerContainer_HighPlayerCount_Left.SetActive(false);
            PlayerContainer_HighPlayerCount_Right.SetActive(false);

            HelperFunctions.DestroyAllChildredImmediately(PlayerContainer_LowPlayerCount);
            
            foreach (MatchParticipant_Player p in Match.PlayerParticipants)
            {
                UI_MatchPlayer row = Instantiate(MatchPlayerPrefab, PlayerContainer_LowPlayerCount.transform);
                row.Init(game, p.Player, game.GetPlayerPoints(p));
                PlayerRows.Add(p.Player, row);
            }
        }

        StartSimulation(stepTime);
    }

    private void StartSimulation(float stepTime)
    {
        StepTime = stepTime;
        IsSimulating = true;
        CurrentSkillIndex = -1;
        SimPlayer = Match.NumPlayerParticipants + 1;
        TimeElapsed = 0;
        CurrentGameRound = null;

        Game.StartGame();
    }

    private void EndSimulation()
    {
        IsSimulating = false;
        Game.SetDone();
        BaseUI.Simulator.Save();
        BaseUI.DisplayMatchOverviewScreen(Match);
    }

    void Update()
    {
        TimeElapsed += Time.deltaTime;
        if(IsSimulating && TimeElapsed >= StepTime)
        {
            TimeElapsed = 0;

            if (CurrentGameRound != null && SimPlayer == CurrentGameRound.NumParticipants)
            {
                EndRound();
                SimPlayer++;
            }
            else if (CurrentGameRound == null || SimPlayer == CurrentGameRound.NumParticipants + 1)
            {
                if (Game.IsGameOver()) EndSimulation();
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
            PlayerRows[participant.Player].PointsText.text = Game.GetPlayerPoints(participant).ToString();
        }

        // Resort rows according to new ranking
        List<Player> playerRanking = Game.GetPlayerRanking().Select(x => x.Player).ToList();
        for (int i = 0; i < playerRanking.Count; i++)
        {
            if (IsUsingBigDisplay)
            {
                if (PlayerRows[playerRanking[i]].RankText != null) PlayerRows[playerRanking[i]].RankText.text = $"{i + 1}.";

                GameObject container = i < NUM_ROWS_IN_HIGH_CONT ? PlayerContainer_HighPlayerCount_Left : PlayerContainer_HighPlayerCount_Right;
                PlayerRows[playerRanking[i]].transform.SetParent(container.transform);
                PlayerRows[playerRanking[i]].transform.SetAsLastSibling();
            }
            else
            {
                PlayerRows[playerRanking[i]].transform.SetSiblingIndex(i);
            }
            
        }

        // Readjust advancement separators
        for(int i = 0; i < AdvancementLimits.Count; i++)
        {
            AdvancementSeparators[i].transform.SetSiblingIndex(AdvancementLimits[i] % NUM_ROWS_IN_HIGH_CONT);
        }
    }

    private void GoToNextSkill()
    {
        // Change attribute
        CurrentSkillIndex++;
        if (CurrentSkillIndex >= Game.Skills.Count) CurrentSkillIndex = 0; // Cycle back to start
        CurrentSkill =  Game.Skills[CurrentSkillIndex];
        
        // Subtitle
        if (Game.IsKnockout) SubtitleText.text = $"Round {Game.Rounds.Count + 1} - {CurrentSkill.LabelCap}";
        else SubtitleText.text = $"Round {CurrentSkillIndex + 1}/{Game.Skills.Count} - {CurrentSkill.LabelCap}";
        SimPlayer = 0;

        // Execute game round
        CurrentGameRound = Game.CreateGameRound(CurrentSkill);
    }
}
