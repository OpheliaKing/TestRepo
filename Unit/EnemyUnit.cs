using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class EnemyUnit : MonoBehaviour
{
    private const string ShootParameter = "Shoot";
    private const string HitParameter = "Hit";
    private const string DieParameter = "Die";
    private const string MoveParameter = "Move";
    private const string IdleState = "Idle";

    [SerializeField] private float _maxHp = 10f;
    [SerializeField] private float _moveSpeed = 1.5f;
    [SerializeField] private long _gold = 1;

    public float Hp { get; private set; }
    public float MaxHp => _maxHp;
    public float MoveSpeed => _moveSpeed;
    public long Gold => _gold;
    public bool IsDead => Hp <= 0f;
    public bool InPool { get; private set; }
    public CharacterPreset Preset { get; private set; }

    private readonly WeaponInfo _weaponInfo = new WeaponInfo();
    private Animator _animator;
    private Coroutine _deathRoutine;

    public WeaponInfo WeaponInfo => _weaponInfo;

    public Task EquipAsync(int tid, WeaponSO weaponTable, ResourceManager resources)
    {
        return WeaponEquip.EquipAsync(transform, _weaponInfo, tid, weaponTable, resources);
    }

    public void Attack(Vector3 targetPosition)
    {
        if (_animator != null)
            _animator.SetTrigger(ShootParameter);

        _weaponInfo.Attack(targetPosition);
    }

    private void Awake()
    {
        Hp = _maxHp;
        _animator = GetComponentInChildren<Animator>(true);
    }

    public void Prepare()
    {
        InPool = false;
        if (_deathRoutine != null)
        {
            StopCoroutine(_deathRoutine);
            _deathRoutine = null;
        }

        Collider body = GetComponent<Collider>();
        if (body != null)
            body.enabled = true;

        gameObject.SetActive(true);
        if (_animator == null)
            return;

        _animator.ResetTrigger(DieParameter);
        _animator.ResetTrigger(ShootParameter);
        _animator.ResetTrigger(HitParameter);
        _animator.SetBool(MoveParameter, false);
        _animator.Play(IdleState, 0, 0f);
        if (_animator.layerCount > 1)
            _animator.Play("Empty", 1, 0f);
    }

    public void MarkInPool()
    {
        InPool = true;
        if (_deathRoutine != null)
        {
            StopCoroutine(_deathRoutine);
            _deathRoutine = null;
        }
    }

    public void ApplyData(EnemyData data)
    {
        if (data == null)
            return;

        Preset = data.Preset;
        _maxHp = Mathf.Max(0f, data.Hp);
        Hp = _maxHp;
        _gold = data.Reward;

        CharacterAppearance appearance = GetComponent<CharacterAppearance>();
        if (appearance == null)
            appearance = gameObject.AddComponent<CharacterAppearance>();

        appearance.Apply(data.Preset);
    }

    public void ApplyDamage(float damage)
    {
        if (IsDead || damage <= 0f)
            return;

        Hp = Mathf.Max(0f, Hp - damage);
        ShowDamage(damage);
        if (!IsDead)
        {
            if (_animator != null)
                _animator.SetTrigger(HitParameter);
            return;
        }

        if (_animator != null)
        {
            _animator.ResetTrigger(HitParameter);
            if (_animator.layerCount > 1)
                _animator.Play("Empty", 1, 0f);
            _animator.SetTrigger(DieParameter);
        }

        PlaySe(PublicVariable.CharacterDieSeAddress);

        Collider body = GetComponent<Collider>();
        if (body != null)
            body.enabled = false;

        MovePanel panel = GetComponentInParent<MovePanel>();
        if (panel != null)
            panel.Detach(transform);

        InGameManager inGame = GameManager.Instance != null ? GameManager.Instance.InGameManager : null;
        if (inGame != null)
            inGame.RemoveEnemy(this);

        PlayerDataManager playerData = GameManager.Instance != null ? GameManager.Instance.PlayerDataManager : null;
        if (playerData != null)
            playerData.AddGold(_gold);

        _deathRoutine = StartCoroutine(ReturnAfterDeath());
    }

    private void ShowDamage(float damage)
    {
        UIManager ui = GameManager.Instance != null ? GameManager.Instance.UIManager : null;
        if (ui == null)
            return;

        Vector3 offset = Vector3.up * 2.1f;
        offset.x += UnityEngine.Random.Range(-0.15f, 0.15f);
        ui.ShowDamage(transform, offset, damage);
    }

    private IEnumerator ReturnAfterDeath()
    {
        yield return null;

        float wait = 0.8f;
        if (_animator != null)
        {
            AnimatorStateInfo state = _animator.IsInTransition(0)
                ? _animator.GetNextAnimatorStateInfo(0)
                : _animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName(DieParameter))
                wait = state.length;
        }

        yield return new WaitForSeconds(wait);

        InGameManager inGame = GameManager.Instance != null ? GameManager.Instance.InGameManager : null;
        if (inGame != null)
            inGame.ReleaseEnemy(this);

        _deathRoutine = null;
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
}
