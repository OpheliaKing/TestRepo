using System;
using UnityEngine;
using UnityEngine.UI;

public class InGameHud : MonoBehaviour
{
    private static Sprite _roundSprite;

    private AllyUnit _player;
    private InGameManager _inGame;
    private PlayerDataManager _playerData;
    private Font _font;
    private GameObject _upgradePage;
    private GameObject _companionPage;
    private Image _upgradeTabImage;
    private Image _companionTabImage;
    private Text _upgradeTabLabel;
    private Text _companionTabLabel;
    private Text _damageValue;
    private Text _fireRateValue;
    private Text _rangeValue;
    private Text _damageCost;
    private Text _fireRateCost;
    private Text _rangeCost;
    private Button _damageButton;
    private Button _fireRateButton;
    private Button _rangeButton;
    private Text _companionValue;
    private Text _companionCost;
    private Text _goldValue;
    private Button _addCompanionButton;

    private readonly Color _sheet = new Color(0.11f, 0.12f, 0.1f, 0.94f);
    private readonly Color _card = new Color(0.2f, 0.22f, 0.16f, 1f);
    private readonly Color _tabOn = new Color(0.93f, 0.78f, 0.38f, 1f);
    private readonly Color _tabOff = new Color(0.16f, 0.17f, 0.14f, 1f);
    private readonly Color _ink = new Color(0.15f, 0.12f, 0.06f, 1f);
    private readonly Color _paper = new Color(0.96f, 0.94f, 0.88f, 1f);
    private readonly Color _muted = new Color(0.74f, 0.71f, 0.6f, 1f);
    private readonly Color _upgrade = new Color(0.86f, 0.48f, 0.18f, 1f);
    private readonly Color _companion = new Color(0.34f, 0.55f, 0.3f, 1f);

    public static InGameHud Open(RectTransform parent, AllyUnit player, InGameManager inGame)
    {
        var root = new GameObject("InGameHud", typeof(RectTransform));
        root.transform.SetParent(parent, false);
        var rect = root.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var hud = root.AddComponent<InGameHud>();
        hud._player = player;
        hud._inGame = inGame;
        hud.BindPlayerData();
        hud.Build(rect);
        hud.ShowUpgrade();
        hud.Refresh();
        return hud;
    }

    private void Build(RectTransform root)
    {
        _font = LoadFont();
        var goldChip = CreateImage("GoldChip", root, _sheet);
        var goldRect = goldChip.rectTransform;
        goldRect.anchorMin = new Vector2(0.5f, 1f);
        goldRect.anchorMax = new Vector2(0.5f, 1f);
        goldRect.pivot = new Vector2(0.5f, 1f);
        goldRect.sizeDelta = new Vector2(460f, 96f);
        goldRect.anchoredPosition = new Vector2(0f, -36f);

        _goldValue = CreateText("Gold", goldChip.transform, "골드 0", 40, _tabOn, TextAnchor.MiddleCenter);
        Stretch(_goldValue.rectTransform);

        var sheet = CreateImage("Sheet", root, _sheet);
        var sheetRect = sheet.rectTransform;
        sheetRect.anchorMin = new Vector2(0f, 0f);
        sheetRect.anchorMax = new Vector2(1f, 0f);
        sheetRect.pivot = new Vector2(0.5f, 0f);
        sheetRect.offsetMin = new Vector2(28f, 24f);
        sheetRect.offsetMax = new Vector2(-28f, 690f);

        var tabs = CreateRect("Tabs", sheetRect);
        tabs.anchorMin = new Vector2(0f, 1f);
        tabs.anchorMax = new Vector2(1f, 1f);
        tabs.pivot = new Vector2(0.5f, 1f);
        tabs.offsetMin = new Vector2(22f, -124f);
        tabs.offsetMax = new Vector2(-22f, -22f);

        _upgradeTabImage = CreateButton("UpgradeTab", tabs, "업그레이드", _tabOn, _ink, ShowUpgrade, out _upgradeTabLabel);
        var upgradeRect = _upgradeTabImage.rectTransform;
        upgradeRect.anchorMin = new Vector2(0f, 0f);
        upgradeRect.anchorMax = new Vector2(0.5f, 1f);
        upgradeRect.offsetMin = Vector2.zero;
        upgradeRect.offsetMax = new Vector2(-8f, 0f);

        _companionTabImage = CreateButton("CompanionTab", tabs, "동료", _tabOff, _paper, ShowCompanion, out _companionTabLabel);
        var companionRect = _companionTabImage.rectTransform;
        companionRect.anchorMin = new Vector2(0.5f, 0f);
        companionRect.anchorMax = new Vector2(1f, 1f);
        companionRect.offsetMin = new Vector2(8f, 0f);
        companionRect.offsetMax = Vector2.zero;

        _upgradePage = CreatePage("UpgradePage", sheetRect);
        CreateUpgradeRow(_upgradePage.transform, 0, "공격력", out _damageValue, out _damageCost, out _damageButton, UpgradeDamage);
        CreateUpgradeRow(_upgradePage.transform, 1, "발사 속도", out _fireRateValue, out _fireRateCost, out _fireRateButton, UpgradeFireRate);
        CreateUpgradeRow(_upgradePage.transform, 2, "사거리", out _rangeValue, out _rangeCost, out _rangeButton, UpgradeRange);

        _companionPage = CreatePage("CompanionPage", sheetRect);
        var guide = CreateText("Guide", _companionPage.transform, "함께 싸울 동료를 추가합니다", 34, _paper, TextAnchor.MiddleCenter);
        var guideRect = guide.rectTransform;
        guideRect.anchorMin = new Vector2(0f, 1f);
        guideRect.anchorMax = new Vector2(1f, 1f);
        guideRect.pivot = new Vector2(0.5f, 1f);
        guideRect.offsetMin = new Vector2(24f, -150f);
        guideRect.offsetMax = new Vector2(-24f, -36f);

        var add = CreateButton("AddCompanion", _companionPage.GetComponent<RectTransform>(), "동료 추가", _companion, _paper, AddCompanion, out _companionCost);
        _companionCost.fontSize = 30;
        _addCompanionButton = add.GetComponent<Button>();
        var addRect = add.rectTransform;
        addRect.anchorMin = new Vector2(0.5f, 1f);
        addRect.anchorMax = new Vector2(0.5f, 1f);
        addRect.pivot = new Vector2(0.5f, 1f);
        addRect.sizeDelta = new Vector2(520f, 120f);
        addRect.anchoredPosition = new Vector2(0f, -190f);

        _companionValue = CreateText("CompanionCount", _companionPage.transform, "동료 0", 32, _muted, TextAnchor.MiddleCenter);
        var countRect = _companionValue.rectTransform;
        countRect.anchorMin = new Vector2(0f, 1f);
        countRect.anchorMax = new Vector2(1f, 1f);
        countRect.pivot = new Vector2(0.5f, 1f);
        countRect.offsetMin = new Vector2(24f, -390f);
        countRect.offsetMax = new Vector2(-24f, -330f);
    }

    private GameObject CreatePage(string name, RectTransform sheet)
    {
        var page = CreateRect(name, sheet);
        page.anchorMin = Vector2.zero;
        page.anchorMax = Vector2.one;
        page.offsetMin = new Vector2(22f, 22f);
        page.offsetMax = new Vector2(-22f, -140f);
        return page.gameObject;
    }

    private void CreateUpgradeRow(Transform page, int index, string title, out Text value, out Text costLabel, out Button button, Action onClick)
    {
        var card = CreateImage(title + "Card", page, _card);
        var rect = card.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(0f, -148f - index * 162f);
        rect.offsetMax = new Vector2(0f, -index * 162f);

        var label = CreateText("Title", card.transform, title, 34, _paper, TextAnchor.MiddleLeft);
        var labelRect = label.rectTransform;
        labelRect.anchorMin = new Vector2(0f, 0.5f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.offsetMin = new Vector2(28f, 0f);
        labelRect.offsetMax = new Vector2(-250f, -8f);

        value = CreateText("Value", card.transform, string.Empty, 28, _muted, TextAnchor.MiddleLeft);
        var valueRect = value.rectTransform;
        valueRect.anchorMin = new Vector2(0f, 0f);
        valueRect.anchorMax = new Vector2(1f, 0.5f);
        valueRect.offsetMin = new Vector2(28f, 8f);
        valueRect.offsetMax = new Vector2(-250f, 0f);

        var buttonImage = CreateButton("Upgrade", rect, "강화", _upgrade, _paper, onClick, out costLabel);
        costLabel.fontSize = 26;
        button = buttonImage.GetComponent<Button>();
        var buttonRect = buttonImage.rectTransform;
        buttonRect.anchorMin = new Vector2(1f, 0.5f);
        buttonRect.anchorMax = new Vector2(1f, 0.5f);
        buttonRect.pivot = new Vector2(1f, 0.5f);
        buttonRect.sizeDelta = new Vector2(190f, 84f);
        buttonRect.anchoredPosition = new Vector2(-22f, 0f);
    }

    private void ShowUpgrade()
    {
        _upgradePage.SetActive(true);
        _companionPage.SetActive(false);
        PaintTab(_upgradeTabImage, _upgradeTabLabel, true);
        PaintTab(_companionTabImage, _companionTabLabel, false);
    }

    private void ShowCompanion()
    {
        _upgradePage.SetActive(false);
        _companionPage.SetActive(true);
        PaintTab(_upgradeTabImage, _upgradeTabLabel, false);
        PaintTab(_companionTabImage, _companionTabLabel, true);
        Refresh();
    }

    private void PaintTab(Image image, Text label, bool selected)
    {
        image.color = selected ? _tabOn : _tabOff;
        label.color = selected ? _ink : _paper;
    }

    private void BindPlayerData()
    {
        _playerData = GameManager.Instance != null ? GameManager.Instance.PlayerDataManager : null;
        if (_playerData != null)
            _playerData.Changed += Refresh;
    }

    private void OnDestroy()
    {
        if (_playerData != null)
            _playerData.Changed -= Refresh;
    }

    private void UpgradeDamage()
    {
        if (_playerData == null)
            return;

        _playerData.UpgradeDamage();
    }

    private void UpgradeFireRate()
    {
        if (_playerData == null)
            return;

        _playerData.UpgradeFireRate();
    }

    private void UpgradeRange()
    {
        if (_playerData == null)
            return;

        _playerData.UpgradeRange();
    }

    private async void AddCompanion()
    {
        if (_inGame == null || _playerData == null || _inGame.CompanionCount >= _inGame.MaxCompanionCount)
            return;

        if (_playerData.Gold < PlayerDataManager.GetCompanionCost(_inGame.CompanionCount))
            return;

        if (_addCompanionButton != null)
            _addCompanionButton.interactable = false;

        await _inGame.AddCompanionAsync();
        Refresh();
    }

    private void Refresh()
    {
        if (_player == null || _playerData == null)
            return;

        WeaponInfo weapon = _player.WeaponInfo;
        _damageValue.text = $"Lv.{_playerData.DamageLevel}   {weapon.Damage:0.#}";
        _fireRateValue.text = $"Lv.{_playerData.FireRateLevel}   초당 {weapon.FireRate:0.#}";
        _rangeValue.text = $"Lv.{_playerData.RangeLevel}   {_player.AttackRange:0.#}";
        PaintUpgrade(_damageCost, _damageButton, _playerData.DamageLevel);
        PaintUpgrade(_fireRateCost, _fireRateButton, _playerData.FireRateLevel);
        PaintUpgrade(_rangeCost, _rangeButton, _playerData.RangeLevel);
        int companions = _inGame != null ? _inGame.CompanionCount : 0;
        int maxCompanions = _inGame != null ? _inGame.MaxCompanionCount : 0;
        _companionValue.text = $"동료 {companions}/{maxCompanions}";
        long companionCost = PlayerDataManager.GetCompanionCost(companions);
        if (_companionCost != null)
            _companionCost.text = companions >= maxCompanions ? "동료 추가" : $"동료 추가\n{companionCost:N0}";
        if (_goldValue != null)
            _goldValue.text = $"골드 {_playerData.Gold:N0}";
        if (_addCompanionButton != null)
            _addCompanionButton.interactable = companions < maxCompanions && _playerData.Gold >= companionCost;
    }

    private void PaintUpgrade(Text label, Button button, int level)
    {
        long cost = PlayerDataManager.GetUpgradeCost(level);
        if (label != null)
            label.text = $"강화\n{cost:N0}";
        if (button != null)
            button.interactable = _playerData.Gold >= cost;
    }

    private Image CreateButton(string name, RectTransform parent, string title, Color color, Color textColor, Action onClick, out Text label)
    {
        var image = CreateImage(name, parent, color);
        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = Color.white;
        button.colors = colors;
        button.onClick.AddListener(() => onClick());
        label = CreateText("Label", image.transform, title, 32, textColor, TextAnchor.MiddleCenter);
        Stretch(label.rectTransform);
        return image;
    }

    private Image CreateImage(string name, Transform parent, Color color)
    {
        var image = CreateRect(name, parent).gameObject.AddComponent<Image>();
        image.sprite = RoundSprite();
        image.type = Image.Type.Sliced;
        image.color = color;
        return image;
    }

    private Text CreateText(string name, Transform parent, string value, int size, Color color, TextAnchor anchor)
    {
        var text = CreateRect(name, parent).gameObject.AddComponent<Text>();
        text.font = _font;
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.alignment = anchor;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
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

    private static Sprite RoundSprite()
    {
        if (_roundSprite != null)
            return _roundSprite;

        const int size = 64;
        const int radius = 22;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = 0f;
                float dy = 0f;
                if (x < radius)
                    dx = radius - x;
                else if (x > size - 1 - radius)
                    dx = x - (size - 1 - radius);

                if (y < radius)
                    dy = radius - y;
                else if (y > size - 1 - radius)
                    dy = y - (size - 1 - radius);

                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = distance > radius ? 0f : 1f;
                if (distance > radius - 1.5f && distance <= radius)
                    alpha = Mathf.Clamp01(radius - distance);

                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        _roundSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(radius, radius, radius, radius));
        return _roundSprite;
    }
}
