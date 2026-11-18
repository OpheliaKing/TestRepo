using UnityEngine;

public class MapInfo : MonoBehaviour
{
    [SerializeField] private Transform _playerPoint;
    [SerializeField] private Transform _cameraPoint;
    [SerializeField] private float _spawnDistance = 18f;
    [SerializeField] private float _stopDistance = 5f;

    public Transform PlayerPoint => _playerPoint;
    public Transform CameraPoint => _cameraPoint;
    public float SpawnDistance => _spawnDistance;
    public float StopDistance => _stopDistance;

    private void OnValidate()
    {
        _spawnDistance = Mathf.Max(0f, _spawnDistance);
        _stopDistance = Mathf.Clamp(_stopDistance, 0f, _spawnDistance);
    }

    public Vector3 GetSpawnPosition(Transform player)
    {
        return GetForwardPosition(player, _spawnDistance);
    }

    public Vector3 GetStopPosition(Transform player)
    {
        return GetForwardPosition(player, _stopDistance);
    }

    private static Vector3 GetForwardPosition(Transform player, float distance)
    {
        if (player == null)
            return Vector3.zero;

        return player.position + player.forward * distance;
    }
}
