using System.Collections;
using UnityEngine;

public class CameraManager : ManagerBase
{
    private const float CompanionFov = 70f;
    private const float BackLineFov = 80f;
    private const int BackLineCompanionCount = 3;
    private const float FovDuration = 0.55f;

    private Camera _camera;
    private float _baseFov = 60f;
    private float _targetFov = 60f;
    private Coroutine _fovRoutine;

    public void Setup(Transform point)
    {
        if (point == null)
        {
            Debug.LogError("[CameraManager] 카메라 위치가 없습니다.");
            return;
        }

        _camera = Camera.main;
        if (_camera == null)
        {
            Debug.LogError("[CameraManager] 카메라가 없습니다.");
            return;
        }

        if (_fovRoutine != null)
        {
            StopCoroutine(_fovRoutine);
            _fovRoutine = null;
        }

        _camera.transform.SetPositionAndRotation(point.position, point.rotation);
        _baseFov = _camera.fieldOfView;
        _targetFov = _baseFov;
    }

    public void SetCompanionCount(int companionCount)
    {
        if (_camera == null)
            return;

        float target = _baseFov;
        if (companionCount >= BackLineCompanionCount)
            target = BackLineFov;
        else if (companionCount >= 1)
            target = CompanionFov;

        if (_fovRoutine != null && Mathf.Approximately(_targetFov, target))
            return;

        _targetFov = target;
        if (_fovRoutine != null)
        {
            StopCoroutine(_fovRoutine);
            _fovRoutine = null;
        }

        if (Mathf.Approximately(_camera.fieldOfView, target))
            return;

        _fovRoutine = StartCoroutine(AnimateFov(target));
    }

    private IEnumerator AnimateFov(float target)
    {
        float start = _camera.fieldOfView;
        float elapsed = 0f;
        while (elapsed < FovDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / FovDuration);
            t = t * t * (3f - 2f * t);
            _camera.fieldOfView = Mathf.Lerp(start, target, t);
            yield return null;
        }

        _camera.fieldOfView = target;
        _fovRoutine = null;
    }
}
