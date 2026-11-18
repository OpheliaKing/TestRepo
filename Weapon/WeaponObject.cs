using UnityEngine;

public class WeaponObject : MonoBehaviour
{
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Transform _fireEffectPoint;
    [SerializeField] private string _fireSoundAddress;

    public Transform FirePoint => _firePoint;
    public Transform FireEffectPoint => _fireEffectPoint;
    public string FireSoundAddress => _fireSoundAddress;

    public void Attack(float damage, WeaponData data, Vector3 targetPosition)
    {
        if (data == null || string.IsNullOrEmpty(data.ProjectileAddress))
            return;

        Transform origin = _firePoint != null ? _firePoint : transform;
        PlaySe(_fireSoundAddress);
        FireAsync(damage, data, origin.position, GetYawRotation(origin.position, targetPosition));
    }

    private static void PlaySe(string address)
    {
        if (string.IsNullOrEmpty(address))
            return;

        SoundManager sound = GameManager.Instance != null ? GameManager.Instance.SoundManager : null;
        if (sound == null)
            return;

        sound.Play(SoundType.Se, address);
    }

    private static Quaternion GetYawRotation(Vector3 origin, Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - origin;
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.0001f)
            return Quaternion.identity;

        return Quaternion.LookRotation(direction, Vector3.up);
    }

    private async void FireAsync(float damage, WeaponData data, Vector3 position, Quaternion rotation)
    {
        InGameManager inGame = GameManager.Instance != null ? GameManager.Instance.InGameManager : null;
        if (inGame == null)
            return;

        ProjectileObject projectile = await inGame.GetProjectileAsync(
            data.Tid,
            data.ProjectileAddress,
            position,
            rotation);
        if (projectile == null)
            return;

        projectile.Launch(damage);
    }
}
