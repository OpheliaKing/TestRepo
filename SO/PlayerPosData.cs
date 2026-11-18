using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerPos", menuName = "Player/Pos")]
public class PlayerPosData : ScriptableObject
{
    [SerializeField] private Vector3[] _positions = Array.Empty<Vector3>();

    public int Count => _positions != null ? _positions.Length : 0;

    public Vector3 Get(int index)
    {
        return _positions[index];
    }
}
