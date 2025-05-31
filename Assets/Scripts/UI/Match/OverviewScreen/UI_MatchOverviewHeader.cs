using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_MatchOverviewHeader : MonoBehaviour
{
    [Header("Elements")]
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI TournamentText;
    public TextMeshProUGUI DateText;

    public Button BackButton;
    public Button SimulateButton;
    public Button SimulateFastButton;

    public void DisplayMatch(UI_Base baseUI, Match match)
    {
        // Reset button listeners
        BackButton.onClick.RemoveAllListeners();
        SimulateButton.onClick.RemoveAllListeners();
        SimulateFastButton.onClick.RemoveAllListeners();

        // Headers
        TitleText.text = match.Name;
        TournamentText.text = match.Tournament.Name;
        DateText.text = match.DateString;

        BackButton.onClick.AddListener(() => baseUI.DisplayTournament(match.Tournament));

        if (match.CanStartNextGame())
        {
            SimulateButton.gameObject.SetActive(true);
            SimulateButton.onClick.AddListener(() => match.SimulateNextGame(stepTime: 1.5f));
            SimulateFastButton.gameObject.SetActive(true);
            SimulateFastButton.onClick.AddListener(() => match.SimulateNextGame(stepTime: 0.1f));
        }
        else
        {
            SimulateButton.gameObject.SetActive(false);
            SimulateFastButton.gameObject.SetActive(false);
        }
    }
}
