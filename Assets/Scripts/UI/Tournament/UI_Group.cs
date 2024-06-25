using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UI_Group : MonoBehaviour
{
    private Match Match;

    [Header("Elements")]
    public Text TitleText;
    public Button SimulateButton;
    public GameObject PlayerContainer;

    [Header("Prefabs")]
    public UI_GroupPlayer PlayerPrefab;

    public void Init(UI_Base baseUI, Match match)
    {
        Match = match;

        SimulateButton.onClick.AddListener(() => baseUI.DisplayMatchScreen(match));
        SimulateButton.gameObject.SetActive(match.CanSimulate());

        TitleText.text = match.Name;

        List<MatchParticipant> participantRanking = match.Participants.OrderByDescending(x => x.TotalScore).ToList();
        foreach(MatchParticipant p in participantRanking)
        {
            UI_GroupPlayer groupPlayer = Instantiate(PlayerPrefab, PlayerContainer.transform);
            groupPlayer.Init(match, p);
        }
    }
}
