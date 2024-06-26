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

        foreach(MatchParticipant p in match.Ranking)
        {
            UI_GroupPlayer groupPlayer = Instantiate(PlayerPrefab, PlayerContainer.transform);
            groupPlayer.Init(match, p);
        }
    }
}
