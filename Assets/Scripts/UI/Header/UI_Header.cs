using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Header : MonoBehaviour
{
    [HideInInspector]
    public UI_Base BaseUI;

    [Header("Elements")]
    public Text TimeText;
    public Button NextDayButton;
    public Button SettingsButton;
    public GameObject SettingsSubmenu;
    public Button SaveButton;

    [Header("Prefabs")]
    public GameObject SubmenuOption;

    [Header("State")]
    public GameObject ActiveSubmenu;

    // Start is called before the first frame update
    public void Init(UI_Base baseUI)
    {
        BaseUI = baseUI;
        InitSettingsSubmenu();
        NextDayButton.onClick.AddListener(() => baseUI.Simulator.GoToNextDay());
        SaveButton.onClick.AddListener(() => BaseUI.Simulator.Save());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateTime(int season, int quarter, int day)
    {
        TimeText.text = Database.GetQuarterName(quarter) + " " + day + ", Season " + season;
        NextDayButton.gameObject.SetActive(BaseUI.Simulator.CanGoToNextDay());
    }

    private void InitSettingsSubmenu()
    {
        SettingsButton.onClick.AddListener(() => ToggleSubmenu(SettingsSubmenu));
        AddSubmenuOption(SettingsSubmenu, "Add Random Player", () => OpenPopupThroughSubmenu(PopupType.AddPlayer));
        AddSubmenuOption(SettingsSubmenu, "Shuffle Skills", ShuffleSkills);
        AddSubmenuOption(SettingsSubmenu, "Schedule Tournament", () => OpenPopupThroughSubmenu(PopupType.ScheduleTournament));
        SettingsSubmenu.SetActive(false);
    }

    private void ToggleSubmenu(GameObject submenu)
    {
        if(submenu == ActiveSubmenu)
        {
            CloseSubmenu();
        }
        else
        {
            ActiveSubmenu = submenu;
            submenu.SetActive(true);
        }
    }

    private void CloseSubmenu()
    {
        if(ActiveSubmenu != null) ActiveSubmenu.SetActive(false);
        ActiveSubmenu = null;
    }

    private void ShuffleSkills()
    {
        BaseUI.Simulator.ShuffleSkillsAndAttributes();
        BaseUI.Simulator.Save();
        CloseSubmenu();
    }

    private void AddSubmenuOption(GameObject submenu, string text, Action action)
    {
        GameObject option = Instantiate(SubmenuOption, submenu.transform);
        option.GetComponentInChildren<Text>().text = text;
        option.GetComponent<Button>().onClick.AddListener(delegate { action(); });
    }

    private void OpenPopupThroughSubmenu(PopupType type)
    {
        CloseSubmenu();
        BaseUI.Popup.ShowPopup(type);
    }
}
