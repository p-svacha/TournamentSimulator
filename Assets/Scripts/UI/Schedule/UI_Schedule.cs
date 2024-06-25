using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Schedule : MonoBehaviour
{
    [Header("Elements")]
    public GameObject ListContainer;

    [Header("Prefabs")]
    public UI_ScheduleElement ListElement;

    public void UpdateList(UI_Base baseUI, List<Tournament> tournaments)
    {
        foreach (Transform t in ListContainer.transform) Destroy(t.gameObject);

        foreach (Tournament t in tournaments)
        {
            UI_ScheduleElement elem = Instantiate(ListElement, ListContainer.transform);
            elem.Init(baseUI, t);
        }
    }
}
