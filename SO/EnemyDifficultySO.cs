using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDifficulty", menuName = "Enemy/Difficulty")]
public class EnemyDifficultySO : ScriptableObject
{
    [SerializeField] private EnemyDifficultyData[] _difficulties = Array.Empty<EnemyDifficultyData>();

    public EnemyDifficultyData[] Difficulties => _difficulties;

    public bool TryGet(int difficulty, out EnemyDifficultyData data)
    {
        for (int i = 0; i < _difficulties.Length; i++)
        {
            EnemyDifficultyData entry = _difficulties[i];
            if (entry == null || entry.Difficulty != difficulty)
                continue;

            data = entry;
            return true;
        }

        data = null;
        return false;
    }
}

[Serializable]
public class EnemyDifficultyData
{
    [SerializeField] private int _difficulty = 1;
    [SerializeField] private EnemyData[] _enemies = Array.Empty<EnemyData>();

    public int Difficulty => _difficulty;
    public EnemyData[] Enemies => _enemies;
}

[Serializable]
public class EnemyData
{
    [SerializeField] private CharacterPreset _preset = CharacterPreset.Robber;
    [SerializeField] private float _hp = 10f;
    [SerializeField] private long _reward = 1;

    public CharacterPreset Preset => _preset;
    public float Hp => _hp;
    public long Reward => _reward;
}
