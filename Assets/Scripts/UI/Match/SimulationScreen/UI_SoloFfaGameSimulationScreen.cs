using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UI_SoloFfaGameSimulationScreen : UI_Screen
{
    private SoloGame Game;
    private SoloMatch Match => Game.Match;

    [Header("Elements")]
    public Text TitleText;
    public Text ProgressText;
    public Text AttributeText;
    public GameObject PlayerContainer;

    [Header("Prefabs")]
    public UI_MatchPlayer MatchPlayerPrefab;
    private Dictionary<Player, UI_MatchPlayer> PlayerRows;

    // Simulation
    private bool IsSimulating;
    private int CurrentSkillIndex;
    private SkillDef CurrentSkill;
    private int SimPlayer;
    private float TimeElapsed;
    private float StepTime;

    private SoloGameRound CurrentGameRound;

    public void DisplayAndSimulateGame(SoloGame game, float stepTime)
    {
        Game = game;

        TitleText.text = game.Label;
        ProgressText.text = "";
        AttributeText.text = "";

        for (int i = 1; i < PlayerContainer.transform.childCount; i++) GameObject.Destroy(PlayerContainer.transform.GetChild(i).gameObject);

        PlayerRows = new Dictionary<Player, UI_MatchPlayer>();
        foreach (MatchParticipant_Player p in Match.PlayerParticipants)
        {
            UI_MatchPlayer row = Instantiate(MatchPlayerPrefab, PlayerContainer.transform);
            row.Init(game, p.Player, game.GetPlayerPoints(p));
            PlayerRows.Add(p.Player, row);
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

            if(SimPlayer == Match.NumPlayerParticipants)
            {
                EndRound();
                SimPlayer++;
            }
            else if(SimPlayer == Match.NumPlayerParticipants + 1)
            {
                if (CurrentSkillIndex == Game.Skills.Count - 1) EndSimulation();
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
        for (int i = 0; i < playerRanking.Count; i++) PlayerRows[playerRanking[i]].transform.SetSiblingIndex(i + 1);
    }

    private void GoToNextSkill()
    {
        // Change attribute
        CurrentSkillIndex++;
        CurrentSkill =  Game.Skills[CurrentSkillIndex];
        ProgressText.text = (CurrentSkillIndex + 1) + "/" + Game.Skills.Count;
        AttributeText.text = CurrentSkill.LabelCap;
        SimPlayer = 0;

        // Execute game round
        CurrentGameRound = Game.CreateGameRound(CurrentSkill);
    }
}
