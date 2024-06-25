using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Popup : MonoBehaviour
{
    public UI_Base BaseUI;

    [Header("Elements")]
    public Text TitleText;
    public Button CloseButton;
    public Button OkButton;
    public Button AbortButton;
    public GameObject ContentContainer;

    [Header("Popups")]
    public Popup_AddPlayer Popup_AddPlayer;
    public Popup_ScheduleTournament Popup_ScheduleTournament;

    public PopupContent ActivePopupContent;

    void Start()
    {
        CloseButton.onClick.AddListener(ClosePopup);
        AbortButton.onClick.AddListener(ClosePopup);
    }

    public PopupContent GetContent(PopupType type)
    {
        if (type == PopupType.AddPlayer) return Popup_AddPlayer;
        if (type == PopupType.ScheduleTournament) return Popup_ScheduleTournament;
        throw new System.Exception("no popup content defined for type " + type.ToString());
    }

    public void ShowPopup(PopupType type)
    {
        gameObject.SetActive(true);
        if (ActivePopupContent != null) ActivePopupContent.gameObject.SetActive(false);
        ActivePopupContent = GetContent(type);
        ActivePopupContent.gameObject.SetActive(true);

        TitleText.text = ActivePopupContent.PopupTitle;
        OkButton.onClick.RemoveAllListeners();
        OkButton.onClick.AddListener(() => { ActivePopupContent.OnOkClick(BaseUI.Simulator); ClosePopup(); });

        ActivePopupContent.OnInitShow();
    }

    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }

}
