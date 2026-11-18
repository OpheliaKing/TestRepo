using System;
using UnityEngine;

[Serializable]
public class PlayerInfo
{
    private const string SaveKey = "PlayerInfo";

    [SerializeField] private int _damageLevel = 1;
    [SerializeField] private int _fireRateLevel = 1;
    [SerializeField] private int _rangeLevel = 1;
    [SerializeField] private float _damageUpgrade;
    [SerializeField] private float _fireRateUpgrade;
    [SerializeField] private float _rangeUpgrade;
    [SerializeField] private int _companionCount;
    [SerializeField] private long _gold;
    [SerializeField] private int _difficulty = PublicVariable.DefaultDifficulty;

    public int DamageLevel => _damageLevel;
    public int FireRateLevel => _fireRateLevel;
    public int RangeLevel => _rangeLevel;
    public float DamageUpgrade => _damageUpgrade;
    public float FireRateUpgrade => _fireRateUpgrade;
    public float RangeUpgrade => _rangeUpgrade;
    public int CompanionCount => _companionCount;
    public long Gold => _gold;
    public int Difficulty => _difficulty;

    public bool TrySpendGold(long amount)
    {
        if (amount < 0 || _gold < amount)
            return false;

        _gold -= amount;
        return true;
    }

    public void AddGold(long amount)
    {
        if (amount <= 0)
            return;

        if (_gold > long.MaxValue - amount)
            _gold = long.MaxValue;
        else
            _gold += amount;
    }

    public void UpgradeDamage()
    {
        _damageUpgrade += 1f;
        _damageLevel++;
    }

    public void UpgradeFireRate()
    {
        _fireRateUpgrade += 0.5f;
        _fireRateLevel++;
    }

    public void UpgradeRange()
    {
        _rangeUpgrade += 2f;
        _rangeLevel++;
    }

    public void SetCompanionCount(int count)
    {
        _companionCount = Mathf.Max(0, count);
    }

    public void SetDifficulty(int difficulty)
    {
        _difficulty = Mathf.Max(1, difficulty);
    }

    public void SaveLocal()
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(this));
        PlayerPrefs.Save();
    }

    public static void DeleteLocal()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }

    public static PlayerInfo LoadLocal()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
            return new PlayerInfo();

        string json = PlayerPrefs.GetString(SaveKey, string.Empty);
        if (string.IsNullOrEmpty(json))
            return new PlayerInfo();

        PlayerInfo info = JsonUtility.FromJson<PlayerInfo>(json);
        if (info == null)
            return new PlayerInfo();

        info.Clamp();
        return info;
    }

    private void Clamp()
    {
        _damageLevel = Mathf.Max(1, _damageLevel);
        _fireRateLevel = Mathf.Max(1, _fireRateLevel);
        _rangeLevel = Mathf.Max(1, _rangeLevel);
        _damageUpgrade = Mathf.Max(0f, _damageUpgrade);
        _fireRateUpgrade = Mathf.Max(0f, _fireRateUpgrade);
        _rangeUpgrade = Mathf.Max(0f, _rangeUpgrade);
        _companionCount = Mathf.Max(0, _companionCount);
        _difficulty = Mathf.Max(1, _difficulty);
        if (_gold < 0)
            _gold = 0;
    }
}
