using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_TeamTooltip_TeamPlayerRow : MonoBehaviour
{
    [Header("Elements")]
    public TextMeshProUGUI KeyText;
    public TextMeshProUGUI ValueText;

    public void Init(Player player, int appearances)
    {
        KeyText.text = player.LastName;
        ValueText.text = appearances.ToString();
    }
}
