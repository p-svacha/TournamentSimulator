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
        if (m.TeamParticipants.Count == 0) InitEmpty();
        else if (m.TeamParticipants.Count == 1) DisplayMatch(m.TeamParticipants[0]);
        else DisplayMatch(m, m.TeamParticipants[0], m.TeamParticipants[1]);
    }

    public void DisplayMatch(TeamMatch match, MatchParticipant_Team team1, MatchParticipant_Team team2)
    {
        Team1Flag.enabled = true;
        Team1Flag.sprite = team1.Team.FlagBig;
        Team1NameText.text = team1.Team.Name;
        Team2Flag.enabled = true;
        Team2Flag.sprite = team2.Team.FlagBig;
        Team2NameText.text = team2.Team.Name;
        ScoreText.text = match.IsDone ? match.GetTeamMatchScore(team1) + " : " + match.GetTeamMatchScore(team2) : " : ";
    }

    private void InitEmpty()
    {
        Team1Flag.enabled = false;
        Team1NameText.text = "";
        Team2Flag.enabled = false;
        Team2NameText.text = "";
        ScoreText.text = " : ";
    }

    public void DisplayMatch(MatchParticipant_Team team1)
    {
        Team1Flag.enabled = true;
        Team1Flag.sprite = team1.Team.FlagBig;
        Team1NameText.text = team1.Team.Name;
        Team2Flag.enabled = false;
        Team2NameText.text = "";
        ScoreText.text = ":";
    }
}
