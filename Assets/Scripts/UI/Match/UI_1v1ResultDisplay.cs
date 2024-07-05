using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_1v1ResultDisplay : MonoBehaviour
{
    [Header("Elements")]
    public Image Team1Flag;
    public TextMeshProUGUI Team1NameText;
    public Image Team2Flag;
    public TextMeshProUGUI Team2NameText;
    public TextMeshProUGUI ScoreText;

    public void DisplayMatch(TeamMatch m)
    {
        DisplayMatch(m.TeamParticipants[0], m.TeamParticipants[1]);
    }

    public void DisplayMatch(MatchParticipant_Team team1, MatchParticipant_Team team2)
    {
        Team1Flag.sprite = team1.Team.Image;
        Team1NameText.text = team1.Team.Name;
        Team2Flag.sprite = team2.Team.Image;
        Team2NameText.text = team2.Team.Name;
        ScoreText.text = team1.TotalPoints + " : " + team2.TotalPoints;
    }
}
