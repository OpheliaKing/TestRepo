using UnityEngine;

public class HitEffectObject : MonoBehaviour
{
    private ParticleSystem[] _particles;
    private float _duration = 1f;
    private float _life;
    private bool _playing;

    public bool InPool { get; private set; }

    private void Awake()
    {
        Cache();
    }

    public void Prepare()
    {
        InPool = false;
        _playing = false;
        _life = 0f;
    }

    public void MarkInPool()
    {
        InPool = true;
        _playing = false;
    }

    public void Play(Vector3 position, Quaternion rotation)
    {
        if (_particles == null)
            Cache();

        InPool = false;
        _playing = true;
        _life = _duration;
        transform.SetPositionAndRotation(position, rotation);
        gameObject.SetActive(true);
        if (_particles == null)
            return;

        for (int i = 0; i < _particles.Length; i++)
        {
            if (_particles[i] == null)
                continue;

            _particles[i].Clear();
            _particles[i].Play();
        }
    }

    private void Update()
    {
        if (!_playing || InPool)
            return;

        _life -= Time.deltaTime;
        if (_life > 0f)
            return;

        _playing = false;
        InGameManager inGame = GameManager.Instance != null ? GameManager.Instance.InGameManager : null;
        if (inGame == null)
        {
            gameObject.SetActive(false);
            return;
        }

        inGame.ReleaseHitEffect(this);
    }

    private void Cache()
    {
        _particles = GetComponentsInChildren<ParticleSystem>(true);
        float longest = 1f;
        if (_particles == null)
            return;

        for (int i = 0; i < _particles.Length; i++)
        {
            if (_particles[i] == null)
                continue;

            ParticleSystem.MainModule main = _particles[i].main;
            float life = main.duration;
            ParticleSystem.MinMaxCurve lifetime = main.startLifetime;
            life += lifetime.mode == ParticleSystemCurveMode.Constant ? lifetime.constant : lifetime.constantMax;
            if (life > longest)
                longest = life;
        }

        _duration = longest;
    }
}
