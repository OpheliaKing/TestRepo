using System.Threading.Tasks;
using UnityEngine;

public class AllyUnit : MonoBehaviour
{
    private const string ShootParameter = "Shoot";

    [SerializeField] private float _attackRange = 20f;

    private readonly WeaponInfo _weaponInfo = new WeaponInfo();
    private Animator _animator;

    public WeaponInfo WeaponInfo => _weaponInfo;
    public float AttackRange
    {
        get
        {
            PlayerDataManager playerData = GameManager.Instance != null ? GameManager.Instance.PlayerDataManager : null;
            return playerData != null ? playerData.CurrentRange(_attackRange) : _attackRange;
        }
    }

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>(true);
    }

    private void OnValidate()
    {
        _attackRange = Mathf.Max(0f, _attackRange);
    }

    private void Update()
    {
        if (!_weaponInfo.CanAttack)
            return;

        InGameManager inGame = GameManager.Instance != null ? GameManager.Instance.InGameManager : null;
        if (inGame == null)
            return;

        EnemyUnit target = inGame.FindNearestEnemy(transform.position, AttackRange);
        if (target == null)
            return;

        Debug.Log($"[AllyUnit] 공격. 대상 {target.name}, 데미지 {_weaponInfo.Damage}");
        Attack(target.transform.position);
    }

    public Task EquipAsync(int tid, WeaponSO weaponTable, ResourceManager resources)
    {
        return WeaponEquip.EquipAsync(transform, _weaponInfo, tid, weaponTable, resources);
    }

    public void Attack(Vector3 targetPosition)
    {
        LookAtTarget(targetPosition);

        if (_animator != null)
            _animator.SetTrigger(ShootParameter);

        _weaponInfo.Attack(targetPosition);
    }

    private void LookAtTarget(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }
}
