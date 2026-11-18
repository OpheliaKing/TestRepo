using System.Collections.Generic;
using UnityEngine;

public class WeaponInfo
{
    private readonly Dictionary<int, WeaponObject> _cached = new();
    private WeaponData _data;
    private WeaponObject _weaponObject;
    private float _nextFireTime;
    private int _shotCount;

    public WeaponData Data => _data;
    public WeaponObject Current => _weaponObject;
    public float Damage => CurrentDamage(_data != null ? _data.BaseAttack : 0f);
    public float FireRate => CurrentFireRate(_data != null ? _data.BaseFireRate : 0f);
    public int ShotCount => _shotCount;
    public bool CanAttack => _weaponObject != null && Damage > 0f && FireRate > 0f && Time.time >= _nextFireTime;

    public void Initialize(WeaponData data)
    {
        if (data == null)
            return;

        bool changed = _data == null || _data.Tid != data.Tid;
        _data = data;
        if (!changed)
            return;

        _shotCount = 0;
        _nextFireTime = 0f;
        _weaponObject = null;
    }

    public bool TryGetCached(int tid, out WeaponObject weaponObject)
    {
        if (_cached.TryGetValue(tid, out weaponObject) && weaponObject != null)
            return true;

        weaponObject = null;
        return false;
    }

    public void Cache(WeaponObject weaponObject)
    {
        if (_data == null || weaponObject == null)
            return;

        _cached[_data.Tid] = weaponObject;
        Activate(weaponObject);
    }

    public void Activate(WeaponObject weaponObject)
    {
        foreach (var pair in _cached)
        {
            if (pair.Value == null)
                continue;

            pair.Value.gameObject.SetActive(pair.Value == weaponObject);
        }

        _weaponObject = weaponObject;
    }

    public void Attack(Vector3 targetPosition)
    {
        float damage = Damage;
        float fireRate = FireRate;
        if (_weaponObject == null || damage <= 0f || fireRate <= 0f)
            return;

        if (Time.time < _nextFireTime)
            return;

        _nextFireTime = Time.time + (1f / fireRate);
        _shotCount++;
        _weaponObject.Attack(damage, _data, targetPosition);
    }

    private static float CurrentDamage(float baseAttack)
    {
        PlayerDataManager playerData = GameManager.Instance != null ? GameManager.Instance.PlayerDataManager : null;
        return playerData != null ? playerData.CurrentDamage(baseAttack) : baseAttack;
    }

    private static float CurrentFireRate(float baseFireRate)
    {
        PlayerDataManager playerData = GameManager.Instance != null ? GameManager.Instance.PlayerDataManager : null;
        return playerData != null ? playerData.CurrentFireRate(baseFireRate) : baseFireRate;
    }
}
