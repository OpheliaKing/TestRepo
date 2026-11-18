using UnityEngine;

public class ProjectileObject : DamageObject
{
    private static readonly RaycastHit[] HitBuffer = new RaycastHit[16];

    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _range = 40f;
    [SerializeField] private LayerMask _unitMask;

    private ParticleSystem[] _particles;
    private float _traveled;
    private bool _launched;

    public int Tid { get; private set; }
    public bool InPool { get; private set; }

    private void Awake()
    {
        if (_unitMask.value == 0)
            _unitMask = LayerMask.GetMask("Unit");

        _particles = GetComponentsInChildren<ParticleSystem>(true);
    }

    private void OnEnable()
    {
        if (_particles == null)
            return;

        for (int i = 0; i < _particles.Length; i++)
        {
            _particles[i].Clear();
            _particles[i].Play();
        }
    }

    public void Prepare(int tid)
    {
        Tid = tid;
        InPool = false;
        _launched = false;
        _traveled = 0f;
    }

    public void MarkInPool()
    {
        InPool = true;
        _launched = false;
    }

    public void Launch(float damage)
    {
        SetDamage(damage);
        _traveled = 0f;
        _launched = true;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!_launched)
            return;

        float step = _speed * Time.deltaTime;
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        if (step > 0f && TryHit(origin, direction, step))
            return;

        transform.position = origin + direction * step;
        _traveled += step;
        if (_traveled >= _range)
            Release();
    }

    private bool TryHit(Vector3 origin, Vector3 direction, float distance)
    {
        int count = Physics.RaycastNonAlloc(
            origin,
            direction,
            HitBuffer,
            distance,
            _unitMask,
            QueryTriggerInteraction.Ignore);

        float closest = float.MaxValue;
        EnemyUnit target = null;
        Vector3 point = origin;
        for (int i = 0; i < count; i++)
        {
            RaycastHit hit = HitBuffer[i];
            if (hit.distance >= closest)
                continue;

            EnemyUnit enemy = hit.collider.GetComponentInParent<EnemyUnit>();
            if (enemy == null || enemy.IsDead)
                continue;

            closest = hit.distance;
            target = enemy;
            point = hit.point;
        }

        if (target == null)
            return false;

        transform.position = point;
        PlaySe(PublicVariable.GunHitSeAddress);
        InGameManager inGame = GameManager.Instance != null ? GameManager.Instance.InGameManager : null;
        if (inGame != null)
            inGame.PlayHitEffect(point, Quaternion.identity);

        target.ApplyDamage(Damage);
        Release();
        return true;
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

    private void Release()
    {
        if (InPool)
            return;

        _launched = false;
        InGameManager inGame = GameManager.Instance != null ? GameManager.Instance.InGameManager : null;
        if (inGame == null)
        {
            gameObject.SetActive(false);
            return;
        }

        inGame.ReleaseProjectile(this);
    }
}
