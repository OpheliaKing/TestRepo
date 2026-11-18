using UnityEngine;

public class SafeArea : MonoBehaviour
{
    private RectTransform _rect;
    private Rect _lastSafeArea;
    private Vector2Int _lastScreen;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        Apply();
    }

    private void Update()
    {
        if (_lastSafeArea == Screen.safeArea
            && _lastScreen.x == Screen.width
            && _lastScreen.y == Screen.height)
            return;

        Apply();
    }

    private void Apply()
    {
        _lastSafeArea = Screen.safeArea;
        _lastScreen = new Vector2Int(Screen.width, Screen.height);
        if (_lastScreen.x <= 0 || _lastScreen.y <= 0)
            return;

        var area = _lastSafeArea;
        _rect.anchorMin = new Vector2(area.xMin / _lastScreen.x, area.yMin / _lastScreen.y);
        _rect.anchorMax = new Vector2(area.xMax / _lastScreen.x, area.yMax / _lastScreen.y);
        _rect.offsetMin = Vector2.zero;
        _rect.offsetMax = Vector2.zero;
    }
}