using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UI_MatchSimulationScreen : UI_Screen
{
    private Match Match;

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

    private Dictionary<Player, PlayerMatchRound> RoundResults;
    private List<Player> AttributeRanking;

    public void DisplayAndSimulateMatch(Match m, float stepTime)
    {
        Match = m;

        TitleText.text = m.Name;
        ProgressText.text = "";
        AttributeText.text = "";

        for (int i = 1; i < PlayerContainer.transform.childCount; i++) GameObject.Destroy(PlayerContainer.transform.GetChild(i).gameObject);

        PlayerRows = new Dictionary<Player, UI_MatchPlayer>();
        foreach (MatchParticipant p in m.Ranking)
        {
            UI_MatchPlayer row = Instantiate(MatchPlayerPrefab, PlayerContainer.transform);
            row.Init(p.Player, p.TotalScore);
            PlayerRows.Add(p.Player, row);
        }

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
        if(IsSimulating && TimeElapsed >= StepTime)
        {
            TimeElapsed = 0;

            if(SimPlayer == Match.NumPlayers)
            {
                EndRound();
                SimPlayer++;
            }
            else if(SimPlayer == Match.NumPlayers + 1)
            {
                if(CurrentSkillIndex == -1) GoToNextSkill();
                else if (CurrentSkillIndex == TournamentSimulator.SkillDefs.Count - 1) EndSimulation();
                else
                {
                    GoToNextSkill();
                }
            }
            else
            {
                Player p = AttributeRanking[SimPlayer];
                PlayerRows[p].DisplayResult(RoundResults[p]);
                SimPlayer++;
            }
        }
    }

    private void EndRound()
    {
        // Clear row texts
        foreach (MatchParticipant p in Match.Participants)
        {
            PlayerRows[p.Player].HideResult();
        }

        // Distribute Points
        foreach (MatchParticipant participant in Match.Participants)
        {
            participant.IncreaseTotalScore(RoundResults[participant.Player].PointsGained);
            PlayerRows[participant.Player].PointsText.text = participant.TotalScore.ToString();
        }

        // Resort rows according to new ranking
        List<Player> playerRanking = Match.PlayerRanking;
        for (int i = 0; i < playerRanking.Count; i++) PlayerRows[playerRanking[i]].transform.SetSiblingIndex(i + 1);

        // Save match round
        Match.Rounds.Add(new MatchRound(CurrentSkill.Id, RoundResults.Values.ToList()));
    }

    private void GoToNextSkill()
    {
        // Change attribute
        CurrentSkillIndex++;
        CurrentSkill = TournamentSimulator.SkillDefs[CurrentSkillIndex];
        ProgressText.text = (CurrentSkillIndex + 1) + "/" + TournamentSimulator.SkillDefs.Count;
        AttributeText.text = TournamentSimulator.SkillDefs[CurrentSkillIndex].DisplayName;
        SimPlayer = 0;

        // Calculate score
        RoundResults = new Dictionary<Player, PlayerMatchRound>();
        foreach (MatchParticipant p in Match.Participants) 
        {
            PlayerMatchRound playerResult = p.Player.GetMatchRoundResult(CurrentSkill);
            RoundResults.Add(p.Player, playerResult);
        }

        // Save all values for the round
        AttributeRanking = RoundResults.OrderBy(x => x.Value.Score).Select(x => x.Key).ToList();
        int lastScore = -1;
        int lastPoints = -1;
        for (int rank = 0; rank < AttributeRanking.Count; rank++)
        {
            Player player = AttributeRanking[rank];
            int score = RoundResults[player].Score;
            int points = Match.PointDistribution[Match.PointDistribution.Count - rank - 1];
            if (score == 0) points = 0;
            else if (score == lastScore) points = lastPoints;
            RoundResults[player].SetPointsGained(points);
            lastScore = score;
            lastPoints = points;
        }
    }
}
