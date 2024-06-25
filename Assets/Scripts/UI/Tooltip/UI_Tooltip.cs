using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Tooltip : MonoBehaviour
{
    public float Width;
    public float Height;
    private const int MouseOffset = 5;
    private const int ScreenEdgeOffset = 20;

    protected void ShowTooltip()
    {
        gameObject.SetActive(true);

        Vector3 position = Input.mousePosition + new Vector3(MouseOffset, MouseOffset, 0);
        Width = GetComponent<RectTransform>().rect.width;
        Height = GetComponent<RectTransform>().rect.height;
        if (position.x + Width > Screen.width) position.x = Screen.width - Width - ScreenEdgeOffset;
        if (position.y - Height < 0) position.y = Height + ScreenEdgeOffset;
        transform.position = position;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
