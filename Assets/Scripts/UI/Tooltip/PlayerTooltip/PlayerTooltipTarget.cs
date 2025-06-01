using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class PlayerTooltipTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private DisciplineDef Discipline;
    private Player Player;

    [HideInInspector] public bool IsFocussed;
    private float Delay = 0.5f;
    [HideInInspector] public float CurrentDelay;

    public void Init(DisciplineDef discipline, Player player)
    {
        Discipline = discipline;
        Player = player;
    }

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
        if (Player == null) return;
        UI_PlayerTooltip.Singleton.Init(Discipline, Player);
    }

    private void HideTooltip()
    {
        IsFocussed = false;
        CurrentDelay = 0;
        UI_PlayerTooltip.Singleton.Hide();
    }
}

