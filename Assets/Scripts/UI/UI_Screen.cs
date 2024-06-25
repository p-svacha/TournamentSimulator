using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Screen : MonoBehaviour
{
    [HideInInspector]
    public UI_Base BaseUI;

    public virtual void Init(UI_Base baseUI)
    {
        BaseUI = baseUI;
    }
}
