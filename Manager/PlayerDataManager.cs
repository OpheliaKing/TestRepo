using System;
using UnityEngine;

public class PlayerDataManager : ManagerBase
{
    private PlayerInfo _info = new PlayerInfo();

    public event Action Changed;

    public PlayerInfo Info => _info;
    public long Gold => _info.Gold;
    public int DamageLevel => _info.DamageLevel;
    public int FireRateLevel => _info.FireRateLevel;
    public int RangeLevel => _info.RangeLevel;
    public int CompanionCount => _info.CompanionCount;
    public int Difficulty => _info.Difficulty;

    public float CurrentDamage(float baseAttack)
    {
        return baseAttack + _info.DamageUpgrade;
    }

    public float CurrentFireRate(float baseFireRate)
    {
        return baseFireRate + _info.FireRateUpgrade;
    }

    public float CurrentRange(float baseRange)
    {
        return baseRange + _info.RangeUpgrade;
    }

    public static long GetUpgradeCost(int level)
    {
        return (long)Mathf.Max(1, level) * PublicVariable.UpgradeCost;
    }

    public static long GetCompanionCost(int companionCount)
    {
        return (long)Mathf.Max(1, companionCount + 1) * PublicVariable.CompanionCost;
    }

    public bool TryPayCompanion(int companionCount)
    {
        return _info.TrySpendGold(GetCompanionCost(companionCount));
    }

    public void Load()
    {
        _info = PlayerInfo.LoadLocal();
        Changed?.Invoke();
    }

    public void AddGold(long amount)
    {
        if (amount <= 0)
            return;

        _info.AddGold(amount);
        Commit();
    }

    public bool UpgradeDamage()
    {
        if (!_info.TrySpendGold(GetUpgradeCost(_info.DamageLevel)))
            return false;

        _info.UpgradeDamage();
        Commit();
        return true;
    }

    public bool UpgradeFireRate()
    {
        if (!_info.TrySpendGold(GetUpgradeCost(_info.FireRateLevel)))
            return false;

        _info.UpgradeFireRate();
        Commit();
        return true;
    }

    public bool UpgradeRange()
    {
        if (!_info.TrySpendGold(GetUpgradeCost(_info.RangeLevel)))
            return false;

        _info.UpgradeRange();
        Commit();
        return true;
    }

    public void SetCompanionCount(int count)
    {
        _info.SetCompanionCount(count);
        Commit();
    }

    public void SetDifficulty(int difficulty)
    {
        _info.SetDifficulty(difficulty);
        Commit();
    }

    private void Commit()
    {
        _info.SaveLocal();
        Changed?.Invoke();
    }
}
