using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class UIManager : ManagerBase
{
    private readonly List<OpenedUI> _stack = new();
    private readonly Stack<DamagePopup> _damagePopups = new();
    private Canvas _canvas;
    private RectTransform _uiRoot;
    private RectTransform _damageRoot;
    private Font _damageFont;
    private int _generation;

    public async Task<UIBase> Show(string address, UIType type)
    {
        if (string.IsNullOrEmpty(address))
        {
            Debug.LogError("[UIManager] address가 비어 있습니다.", this);
            return null;
        }

        var resources = GameManager.Instance != null ? GameManager.Instance.ResourceManager : null;
        if (resources == null)
        {
            Debug.LogError("[UIManager] ResourceManager가 없습니다.", this);
            return null;
        }

        EnsureCanvas();
        var generation = _generation;
        var instance = await resources.InstantiateAsync(address, _uiRoot, startInactive: true);
        if (this == null)
        {
            if (instance != null)
                resources.ReleaseInstance(instance);
            return null;
        }

        if (generation != _generation || instance == null)
        {
            if (instance != null)
                resources.ReleaseInstance(instance);
            return null;
        }

        if (instance.GetComponent<RectTransform>() == null)
        {
            Debug.LogError($"[UIManager] RectTransform이 없습니다. address={address}", instance);
            resources.ReleaseInstance(instance);
            return null;
        }

        var ui = instance.GetComponent<UIBase>();
        if (ui == null)
            ui = instance.AddComponent<UIBase>();

        ui.SetOpenInfo(type, address);
        var opened = new OpenedUI { UI = ui };
        if (type == UIType.Common)
            HideOpenUIs(opened);

        _stack.Add(opened);
        ui.transform.SetAsLastSibling();
        ui.SetVisible(true);
        return ui;
    }

    public UIBase ShowStart(Action onPressed)
    {
        EnsureCanvas();
        RectTransform canvasRect = _canvas.transform as RectTransform;
        StartUI ui = StartUI.Create(canvasRect, onPressed);
        _stack.Add(new OpenedUI { UI = ui });
        ui.transform.SetAsLastSibling();
        ui.SetVisible(true);
        return ui;
    }

    public void Close()
    {
        if (_stack.Count == 0)
            return;

        var opened = _stack[_stack.Count - 1];
        _stack.RemoveAt(_stack.Count - 1);
        RestoreHidden(opened);
        Release(opened.UI);
    }

    public void CloseAll()
    {
        _generation++;
        for (var i = _stack.Count - 1; i >= 0; i--)
            Release(_stack[i].UI);

        _stack.Clear();
    }

    private void HideOpenUIs(OpenedUI opened)
    {
        for (var i = 0; i < _stack.Count; i++)
        {
            var below = _stack[i].UI;
            if (below == null || !below.gameObject.activeSelf)
                continue;

            below.SetVisible(false);
            opened.Hidden.Add(below);
        }
    }

    private static void RestoreHidden(OpenedUI opened)
    {
        if (opened.UI == null || opened.UI.Type != UIType.Common)
            return;

        for (var i = 0; i < opened.Hidden.Count; i++)
        {
            if (opened.Hidden[i] != null)
                opened.Hidden[i].SetVisible(true);
        }
    }

    private void Release(UIBase ui)
    {
        if (ui == null)
            return;

        var resources = GameManager.Instance != null ? GameManager.Instance.ResourceManager : null;
        if (resources == null)
        {
            Destroy(ui.gameObject);
            return;
        }

        resources.ReleaseInstance(ui.gameObject);
    }

    private void EnsureCanvas()
    {
        if (_canvas != null)
            return;

        var canvasObject = new GameObject("UICanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        _canvas = canvasObject.GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 10;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(PublicVariable.ReferenceWidth, PublicVariable.ReferenceHeight);
        scaler.matchWidthOrHeight = 0f;

        var safeObject = new GameObject("SafeArea", typeof(RectTransform), typeof(SafeArea));
        safeObject.transform.SetParent(canvasObject.transform, false);
        _uiRoot = safeObject.GetComponent<RectTransform>();
        _uiRoot.anchorMin = Vector2.zero;
        _uiRoot.anchorMax = Vector2.one;
        _uiRoot.offsetMin = Vector2.zero;
        _uiRoot.offsetMax = Vector2.zero;
        EnsureEventSystem();
        EnsureDamageRoot();
    }

    public void ShowDamage(Transform target, Vector3 worldOffset, float damage)
    {
        if (damage <= 0f)
            return;

        EnsureCanvas();
        DamagePopup popup = RentDamagePopup();
        popup.transform.SetAsLastSibling();
        popup.Play(target, worldOffset, damage);
    }

    private void EnsureDamageRoot()
    {
        if (_damageRoot != null)
            return;

        var root = new GameObject("DamageLayer", typeof(RectTransform));
        root.transform.SetParent(_canvas.transform, false);
        _damageRoot = root.GetComponent<RectTransform>();
        _damageRoot.anchorMin = Vector2.zero;
        _damageRoot.anchorMax = Vector2.one;
        _damageRoot.offsetMin = Vector2.zero;
        _damageRoot.offsetMax = Vector2.zero;
        _damageRoot.SetAsLastSibling();
    }

    private DamagePopup RentDamagePopup()
    {
        while (_damagePopups.Count > 0)
        {
            DamagePopup pooled = _damagePopups.Pop();
            if (pooled != null)
                return pooled;
        }

        if (_damageFont == null)
            _damageFont = LoadDamageFont();

        return DamagePopup.Create(_damageRoot, _damageFont, ReleaseDamagePopup);
    }

    private void ReleaseDamagePopup(DamagePopup popup)
    {
        if (popup == null)
            return;

        _damagePopups.Push(popup);
    }

    private static Font LoadDamageFont()
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

    public RectTransform UiRoot
    {
        get
        {
            EnsureCanvas();
            return _uiRoot;
        }
    }

    private void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
            return;

        var eventObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        eventObject.transform.SetParent(transform, false);
    }

    private sealed class OpenedUI
    {
        public UIBase UI;
        public readonly List<UIBase> Hidden = new();
    }
}
