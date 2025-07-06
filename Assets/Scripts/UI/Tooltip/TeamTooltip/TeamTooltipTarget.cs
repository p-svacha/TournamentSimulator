using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TeamTooltipTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private DisciplineDef Discipline;
    private Team Team;

    [HideInInspector] public bool IsFocussed;
    private float Delay = 0.5f;
    [HideInInspector] public float CurrentDelay;

    public void Init(DisciplineDef discipline, Team team)
    {
        Discipline = discipline;
        Team = team;
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
        if (IsFocussed)
        {
            if (CurrentDelay < Delay) CurrentDelay += Time.deltaTime;
            else ShowTooltip();
        }
    }

    private void ShowTooltip()
    {
        if (Team == null) return;
        UI_TeamTooltip.Instance.Init(Discipline, Team);
    }

    private void HideTooltip()
    {
        IsFocussed = false;
        CurrentDelay = 0;
        UI_TeamTooltip.Instance.Hide();
    }
}
