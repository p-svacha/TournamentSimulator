using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UI_MatchScreen : UI_Screen
{
    private Match Match;

    [Header("Elements")]
    public Button BackButton;
    public Button SimulateButton;
    public Button SimulateFastButton;
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

    private Dictionary<Player, int> RoundScores;
    private Dictionary<Player, int> RoundPoints;
    private List<Player> AttributeRanking;

    public void DisplayMatch(Match m)
    {
        Match = m;

        TitleText.text = m.Name;
        ProgressText.text = "";
        AttributeText.text = "";

        BackButton.onClick.RemoveAllListeners();
        BackButton.onClick.AddListener(() => BaseUI.DisplayTournament(m.Tournament));

        SimulateButton.gameObject.SetActive(true);
        SimulateButton.onClick.AddListener(() => StartSimulation(1.5f));
        SimulateFastButton.gameObject.SetActive(true);
        SimulateFastButton.onClick.AddListener(() => StartSimulation(0.01f));

        for (int i = 1; i < PlayerContainer.transform.childCount; i++) GameObject.Destroy(PlayerContainer.transform.GetChild(i).gameObject);

        PlayerRows = new Dictionary<Player, UI_MatchPlayer>();
        foreach (KeyValuePair<Player, int> kvp in m.GetResult())
        {
            UI_MatchPlayer row = Instantiate(MatchPlayerPrefab, PlayerContainer.transform);
            row.Init(kvp.Key, kvp.Value);
            PlayerRows.Add(kvp.Key, row);
        }
    }

    private void StartSimulation(float stepTime)
    {
        StepTime = stepTime;
        SimulateButton.gameObject.SetActive(false);
        SimulateFastButton.gameObject.SetActive(false);
        BackButton.gameObject.SetActive(false);
        IsSimulating = true;
        CurrentSkillIndex = -1;
        SimPlayer = Match.NumPlayers + 1;
        TimeElapsed = 0;

        // Set elo before match
        foreach (MatchParticipant p in Match.Participants) p.SetPreMatchStats();
    }

    private void EndSimulation()
    {
        IsSimulating = false;
        BackButton.gameObject.SetActive(true);
        Match.SetDone();
        BaseUI.Simulator.Save();
    }

    void Update()
    {
        TimeElapsed += Time.deltaTime;
        if(IsSimulating && TimeElapsed >= StepTime)
        {
            TimeElapsed = 0;

            if(SimPlayer == Match.NumPlayers)
            {
                DistributePoints();
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
                PlayerRows[p].ScoreText.text = RoundScores[p].ToString();
                PlayerRows[p].PlusPointsText.text = "+ " + RoundPoints[p];

                SimPlayer++;
            }
        }
    }

    private void DistributePoints()
    {
        // Clear rows
        foreach (MatchParticipant p in Match.Participants)
        {
            PlayerRows[p.Player].ScoreText.text = "";
            PlayerRows[p.Player].PlusPointsText.text = "";
        }

        SaveMatchRound();
    }

    private void SaveMatchRound()
    {
        List<PlayerMatchRound> roundResults = new List<PlayerMatchRound>();

        if (RoundScores != null)
        {
            foreach (MatchParticipant participant in Match.Participants)
            {
                roundResults.Add(new PlayerMatchRound(participant.Player, RoundScores[participant.Player], RoundPoints[participant.Player], null));
                participant.IncreaseTotalScore(RoundPoints[participant.Player]);
                PlayerRows[participant.Player].PointsText.text = participant.TotalScore.ToString();
            }

            List<Player> playerRanking = Match.Participants.OrderByDescending(x => x.TotalScore).Select(x => x.Player).ToList();
            for (int i = 0; i < playerRanking.Count; i++) PlayerRows[playerRanking[i]].transform.SetSiblingIndex(i + 1);
        }
        Match.Rounds.Add(new MatchRound(CurrentSkill.Id, roundResults));
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
        RoundScores = new Dictionary<Player, int>();
        RoundPoints = new Dictionary<Player, int>();
        foreach (MatchParticipant p in Match.Participants) 
        {
            float baseScore = p.Player.Skills[CurrentSkill.Id];
            int adjustedScore = (int)(RandomGaussian(baseScore - TournamentSimulator.PlayerInconsistency, baseScore + TournamentSimulator.PlayerInconsistency));
            if (adjustedScore < 0) adjustedScore = 0;
            RoundScores.Add(p.Player, adjustedScore);
        }

        // Save all values for the round
        AttributeRanking = RoundScores.OrderBy(x => x.Value).Select(x => x.Key).ToList();
        int lastScore = -1;
        int lastPoints = -1;
        for (int i = 0; i < AttributeRanking.Count; i++)
        {
            Player player = AttributeRanking[i];
            int score = RoundScores[player];
            int points = Match.PointDistribution[Match.PointDistribution.Count - i - 1];
            if (score == 0) points = 0;
            else if (score == lastScore) points = lastPoints;
            RoundPoints.Add(player, points);
            lastScore = score;
            lastPoints = points;
        }
    }

    public static float RandomGaussian(float minValue = 0.0f, float maxValue = 1.0f)
    {
        float u, v, S;

        do
        {
            u = 2.0f * UnityEngine.Random.value - 1.0f;
            v = 2.0f * UnityEngine.Random.value - 1.0f;
            S = u * u + v * v;
        }
        while (S >= 1.0f);

        // Standard Normal Distribution
        float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);

        // Normal Distribution centered between the min and max value
        // and clamped following the "three-sigma rule"
        float mean = (minValue + maxValue) / 2.0f;
        float sigma = (maxValue - mean) / 3.0f;
        return Mathf.Clamp(std * sigma + mean, minValue, maxValue);
    }
}
