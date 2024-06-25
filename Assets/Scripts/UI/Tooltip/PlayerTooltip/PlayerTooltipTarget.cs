using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class PlayerTooltipTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Player Player;

    [HideInInspector] public bool IsFocussed;
    private float Delay = 0.5f;
    [HideInInspector] public float CurrentDelay;

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsFocussed = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    private void Update()
    {
        if(IsFocussed)
        {
            if(CurrentDelay < Delay) CurrentDelay += Time.deltaTime;
            else ShowTooltip();
        }
    }

    private void ShowTooltip()
    {
        UI_PlayerTooltip.Singleton.Init(Player);
    }

    private void HideTooltip()
    {
        IsFocussed = false;
        CurrentDelay = 0;
        UI_PlayerTooltip.Singleton.Hide();
    }
}

