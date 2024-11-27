

using System;
using UnityEngine;
using UnityEngine.UI;

public class ColorSelectUI : MonoBehaviour
{
    public Image border;
    public Image colorImage;

    public Button button;
    public bool IsSelected = false;

    public int ID = 0;
    public Color Color;
    private Action ClickAction;

    void Start()
    {
        border = transform.Find("Border").GetComponent<Image>();
        colorImage = transform.Find("Color").GetComponent<Image>();
        button = transform.Find("Click").GetComponent<Button>();
        border.gameObject.SetActive(IsSelected);
        colorImage.color = Color;
        button.onClick.AddListener(() => { SetSelected(!IsSelected); ClickAction?.Invoke(); });
    }

    public void SetColor(int id, Color color)
    {
        ID = id;
        Color = color;
        if (colorImage != null)
            colorImage.color = color;
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        if (border != null)
            border.gameObject.SetActive(selected);
    }

    public void SetClickAction(Action action)   
    {
        ClickAction = action;
    }
}