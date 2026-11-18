using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Weapon/Weapon")]
public class WeaponSO : ScriptableObject
{
    [SerializeField] private WeaponData[] _weapons = Array.Empty<WeaponData>();

    public WeaponData[] Weapons => _weapons;

    public bool TryGet(int tid, out WeaponData data)
    {
        for (int i = 0; i < _weapons.Length; i++)
        {
            WeaponData weapon = _weapons[i];
            if (weapon == null || weapon.Tid != tid)
                continue;

            data = weapon;
            return true;
        }

        data = null;
        return false;
    }
}

[Serializable]
public class WeaponData
{
    [SerializeField] private int _tid;
    [SerializeField] private float _baseAttack = 1f;
    [SerializeField] private float _baseFireRate = 2f;
    [SerializeField] private string _weaponAddress;
    [SerializeField] private string _projectileAddress;

    public int Tid => _tid;
    public float BaseAttack => _baseAttack;
    public float BaseFireRate => _baseFireRate;
    public string WeaponAddress => _weaponAddress;
    public string ProjectileAddress => _projectileAddress;
}
