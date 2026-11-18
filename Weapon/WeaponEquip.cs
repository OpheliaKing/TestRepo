using System.Threading.Tasks;
using UnityEngine;

public static class WeaponEquip
{
    public static async Task EquipAsync(
        Transform owner,
        WeaponInfo weaponInfo,
        int tid,
        WeaponSO weaponTable,
        ResourceManager resources)
    {
        if (owner == null || weaponInfo == null)
            return;

        if (weaponTable == null || resources == null)
        {
            Debug.LogError($"[{owner.name}] 무기 장착에 필요한 정보가 없습니다.");
            return;
        }

        if (!weaponTable.TryGet(tid, out var data) || data == null)
        {
            Debug.LogError($"[{owner.name}] 무기 tid {tid}가 없습니다.");
            return;
        }

        weaponInfo.Initialize(data);

        if (weaponInfo.TryGetCached(tid, out var cached))
        {
            weaponInfo.Activate(cached);
            return;
        }

        var hand = FindChild(owner, "hand_r");
        if (hand == null)
        {
            Debug.LogError($"[{owner.name}] hand_r이 없습니다.");
            return;
        }

        var instance = await resources.InstantiateAsync(data.WeaponAddress, hand, false);
        if (instance == null)
            return;

        var weaponObject = instance.GetComponent<WeaponObject>();
        if (weaponObject == null)
        {
            Debug.LogError($"[{owner.name}] WeaponObject가 없습니다.");
            resources.ReleaseInstance(instance);
            return;
        }

        weaponInfo.Cache(weaponObject);
    }

    private static Transform FindChild(Transform root, string targetName)
    {
        if (root.name == targetName)
            return root;

        for (int i = 0; i < root.childCount; i++)
        {
            var found = FindChild(root.GetChild(i), targetName);
            if (found != null)
                return found;
        }

        return null;
    }
}
