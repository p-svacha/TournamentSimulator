using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_PlayerList : MonoBehaviour
{
    [Header("Elements")]
    public GameObject ListContainer;

    public void Clear()
    {
        HelperFunctions.DestroyAllChildredImmediately(ListContainer);
    }
}
