using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_Schedule : MonoBehaviour
{
    [Header("Elements")]
    public GameObject ListContainer;

    [Header("Prefabs")]
    public UI_ScheduleElement ListElement;

    public void UpdateList(UI_Base baseUI, int season)
    {
        foreach (Transform t in ListContainer.transform) Destroy(t.gameObject);

        List<Tournament> tournaments = Database.GetTournaments(season);
        List<System.Tuple<Tournament, int>> matchDays = new List<System.Tuple<Tournament, int>>();

        // Aggregate all match days
        foreach (Tournament t in tournaments)
            foreach (int absoluteDay in t.GetMatchDays())
                matchDays.Add(new System.Tuple<Tournament, int>(t, absoluteDay));

        // Sort them
        matchDays = matchDays.OrderBy(x => x.Item2).ToList();

        // Initialize elements
        foreach(System.Tuple<Tournament, int> matchDay in matchDays)
        {
            UI_ScheduleElement elem = Instantiate(ListElement, ListContainer.transform);
            elem.Init(baseUI, matchDay.Item1, matchDay.Item2);
        }
    }
}

