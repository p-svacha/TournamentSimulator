using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerMedal : MonoBehaviour
{
    [Header("Elements")]
    public Image MedalIcon;
    public TextMeshProUGUI MedalText;

    public void Init(MedalInfo info)
    {
        MedalIcon.sprite = ResourceManager.LoadSprite("Icons/medal_" + info.Medal.ToString().ToLower());
        MedalText.text = info.Text;
    }
}
