using System;
using UnityEngine;
using UnityEngine.UI;

public class StartUI : UIBase
{
    public static StartUI Create(RectTransform parent, Action onPressed)
    {
        var root = new GameObject("StartUI", typeof(RectTransform));
        root.transform.SetParent(parent, false);
        var rect = root.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var ui = root.AddComponent<StartUI>();
        ui.SetOpenInfo(UIType.Popup, string.Empty);
        ui.Build(onPressed);
        return ui;
    }

    private void Build(Action onPressed)
    {
        var image = gameObject.AddComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = true;

        var button = gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(() => onPressed?.Invoke());

        var textObject = new GameObject("Label", typeof(RectTransform));
        textObject.transform.SetParent(transform, false);
        var textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0.4f);
        textRect.anchorMax = new Vector2(1f, 0.6f);
        textRect.offsetMin = new Vector2(48f, 0f);
        textRect.offsetMax = new Vector2(-48f, 0f);

        var text = textObject.AddComponent<Text>();
        text.font = LoadFont();
        text.text = "Touch to Start";
        text.fontSize = 52;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.96f, 0.94f, 0.88f, 1f);
        text.raycastTarget = false;
    }

    private static Font LoadFont()
    {
        try
        {
            Font font = Font.CreateDynamicFontFromOSFont("Malgun Gothic", 48);
            if (font != null)
                return font;
        }
        catch (Exception)
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }
}
