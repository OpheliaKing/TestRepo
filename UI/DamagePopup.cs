using System;
using UnityEngine;
using UnityEngine.UI;

public class DamagePopup : MonoBehaviour
{
    private const float Duration = 0.7f;
    private const float Rise = 90f;

    private Text _text;
    private Outline _outline;
    private RectTransform _rect;
    private Transform _target;
    private Vector3 _worldOffset;
    private Vector3 _world;
    private float _elapsed;
    private bool _playing;
    private Action<DamagePopup> _release;
    private Canvas _canvas;

    public static DamagePopup Create(RectTransform parent, Font font, Action<DamagePopup> release)
    {
        var root = new GameObject("DamagePopup", typeof(RectTransform));
        root.transform.SetParent(parent, false);
        var rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(180f, 72f);

        var popup = root.AddComponent<DamagePopup>();
        popup._rect = rect;
        popup._release = release;
        popup._canvas = parent.GetComponentInParent<Canvas>();
        popup._text = root.AddComponent<Text>();
        popup._text.font = font;
        popup._text.fontSize = 42;
        popup._text.alignment = TextAnchor.MiddleCenter;
        popup._text.color = new Color(0.98f, 0.93f, 0.72f, 1f);
        popup._text.horizontalOverflow = HorizontalWrapMode.Overflow;
        popup._text.verticalOverflow = VerticalWrapMode.Overflow;
        popup._text.raycastTarget = false;
        popup._outline = root.AddComponent<Outline>();
        popup._outline.effectColor = new Color(0.12f, 0.08f, 0.04f, 0.95f);
        popup._outline.effectDistance = new Vector2(2f, -2f);
        root.SetActive(false);
        return popup;
    }

    public void Play(Transform target, Vector3 worldOffset, float damage)
    {
        _target = target;
        _worldOffset = worldOffset;
        _world = target != null ? target.position + worldOffset : worldOffset;
        _elapsed = 0f;
        _playing = true;
        _text.text = Format(damage);
        Color color = _text.color;
        color.a = 1f;
        _text.color = color;
        _outline.effectColor = new Color(0.12f, 0.08f, 0.04f, 0.95f);
        gameObject.SetActive(true);
        Place(0f);
    }

    private void LateUpdate()
    {
        if (!_playing)
            return;

        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / Duration);
        Place(t);
        if (t < 1f)
            return;

        _playing = false;
        _target = null;
        gameObject.SetActive(false);
        _release?.Invoke(this);
    }

    private void Place(float t)
    {
        if (_target != null)
            _world = _target.position + _worldOffset;

        Camera camera = Camera.main;
        if (camera == null || _canvas == null)
            return;

        Vector3 screen = camera.WorldToScreenPoint(_world);
        if (screen.z < 0f)
        {
            _text.enabled = false;
            return;
        }

        _text.enabled = true;
        RectTransform canvasRect = _canvas.transform as RectTransform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out Vector2 local))
            return;

        float rise = Rise * Mathf.SmoothStep(0f, 1f, t);
        _rect.anchoredPosition = local + new Vector2(0f, rise);
        float alpha = t < 0.55f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.55f) / 0.45f);
        Color color = _text.color;
        color.a = alpha;
        _text.color = color;
        Color outline = _outline.effectColor;
        outline.a = alpha;
        _outline.effectColor = outline;
    }

    private static string Format(float damage)
    {
        float rounded = Mathf.Round(damage);
        if (Mathf.Abs(damage - rounded) < 0.05f)
            return rounded.ToString("0");

        return damage.ToString("0.#");
    }
}
