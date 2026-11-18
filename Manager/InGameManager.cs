using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class InGameManager : ManagerBase
{
    [SerializeField] private int _enemyCount = 25;
    [SerializeField] private int _difficulty = PublicVariable.DefaultDifficulty;

    private readonly Dictionary<int, Stack<ProjectileObject>> _projectilePools = new();
    private readonly Stack<HitEffectObject> _hitEffectPool = new();
    private readonly Dictionary<int, Stack<EnemyUnit>> _enemyPools = new();
    private readonly List<EnemyUnit> _enemies = new();

    private readonly List<AllyUnit> _allies = new();

    private GameObject _map;
    private MapInfo _mapInfo;
    private PlayerPosData _playerPositions;
    private WeaponSO _weaponTable;
    private MovePanel _movePanel;
    private EnemyDifficultySO _enemyTable;
    private bool _addingCompanion;
    private bool _spawningWave;
    private bool _hasWavePath;
    private Vector3 _spawnPosition;
    private Vector3 _stopPosition;
    private Quaternion _spawnRotation;
    private float _waveSpawnTime = -1f;

    public MapInfo MapInfo => _mapInfo;
    public AllyUnit PlayerUnit => _allies.Count > 0 ? _allies[0] : null;
    public GameObject Player => PlayerUnit != null ? PlayerUnit.gameObject : null;
    public MovePanel MovePanel => _movePanel;
    public int CompanionCount => _allies.Count > 0 ? _allies.Count - 1 : 0;
    public int MaxCompanionCount => _playerPositions != null ? Mathf.Max(0, _playerPositions.Count - 1) : 0;

    public async Task<ProjectileObject> GetProjectileAsync(
        int tid,
        string address,
        Vector3 position,
        Quaternion rotation)
    {
        if (string.IsNullOrEmpty(address))
            return null;

        if (!_projectilePools.TryGetValue(tid, out Stack<ProjectileObject> pool))
        {
            pool = new Stack<ProjectileObject>();
            _projectilePools[tid] = pool;
        }

        while (pool.Count > 0)
        {
            ProjectileObject pooled = pool.Pop();
            if (pooled == null)
                continue;

            pooled.Prepare(tid);
            pooled.transform.SetPositionAndRotation(position, rotation);
            return pooled;
        }

        ResourceManager resources = GameManager.Instance != null ? GameManager.Instance.ResourceManager : null;
        if (resources == null)
            return null;

        GameObject instance = await resources.InstantiateAsync(address, null, true, position);
        if (instance == null)
            return null;

        instance.transform.SetPositionAndRotation(position, rotation);
        ProjectileObject projectile = instance.GetComponent<ProjectileObject>();
        if (projectile == null)
        {
            Debug.LogError($"[InGameManager] ProjectileObject가 없습니다. tid {tid}");
            resources.ReleaseInstance(instance);
            return null;
        }

        projectile.Prepare(tid);
        return projectile;
    }

    public void ReleaseProjectile(ProjectileObject projectile)
    {
        if (projectile == null || projectile.InPool)
            return;

        projectile.MarkInPool();
        projectile.gameObject.SetActive(false);
        if (!_projectilePools.TryGetValue(projectile.Tid, out Stack<ProjectileObject> pool))
        {
            pool = new Stack<ProjectileObject>();
            _projectilePools[projectile.Tid] = pool;
        }

        pool.Push(projectile);
    }

    public void PlayHitEffect(Vector3 position, Quaternion rotation)
    {
        PlayHitEffectAsync(position, rotation);
    }

    public void ReleaseHitEffect(HitEffectObject effect)
    {
        if (effect == null || effect.InPool)
            return;

        effect.MarkInPool();
        effect.gameObject.SetActive(false);
        _hitEffectPool.Push(effect);
    }

    private async void PlayHitEffectAsync(Vector3 position, Quaternion rotation)
    {
        HitEffectObject effect = await GetHitEffectAsync();
        if (effect == null)
            return;

        effect.Play(position, rotation);
    }

    private async Task<HitEffectObject> GetHitEffectAsync()
    {
        while (_hitEffectPool.Count > 0)
        {
            HitEffectObject pooled = _hitEffectPool.Pop();
            if (pooled == null)
                continue;

            pooled.Prepare();
            return pooled;
        }

        ResourceManager resources = GameManager.Instance != null ? GameManager.Instance.ResourceManager : null;
        if (resources == null)
            return null;

        GameObject instance = await resources.InstantiateAsync(PublicVariable.HitEffectAddress, null, true);
        if (instance == null)
            return null;

        HitEffectObject effect = instance.GetComponent<HitEffectObject>();
        if (effect == null)
            effect = instance.AddComponent<HitEffectObject>();

        effect.Prepare();
        return effect;
    }

    public async Task<EnemyUnit> GetEnemyAsync(ResourceManager resources, CharacterPreset preset)
    {
        int key = (int)preset;
        if (!_enemyPools.TryGetValue(key, out Stack<EnemyUnit> pool))
        {
            pool = new Stack<EnemyUnit>();
            _enemyPools[key] = pool;
        }

        while (pool.Count > 0)
        {
            EnemyUnit pooled = pool.Pop();
            if (pooled == null)
                continue;

            pooled.Prepare();
            return pooled;
        }

        if (resources == null)
            return null;

        GameObject instance = await resources.InstantiateAsync(PublicVariable.EnemyPrefabAddress, null, true);
        if (instance == null)
            return null;

        EnemyUnit enemy = instance.GetComponent<EnemyUnit>();
        if (enemy == null)
        {
            Debug.LogError("[InGameManager] EnemyUnit이 없습니다.");
            resources.ReleaseInstance(instance);
            return null;
        }

        enemy.Prepare();
        return enemy;
    }

    public void ReleaseEnemy(EnemyUnit enemy)
    {
        if (enemy == null || enemy.InPool)
            return;

        enemy.MarkInPool();
        enemy.gameObject.SetActive(false);
        int key = (int)enemy.Preset;
        if (!_enemyPools.TryGetValue(key, out Stack<EnemyUnit> pool))
        {
            pool = new Stack<EnemyUnit>();
            _enemyPools[key] = pool;
        }

        pool.Push(enemy);
    }

    public void AddEnemy(EnemyUnit enemy)
    {
        if (enemy == null || _enemies.Contains(enemy))
            return;

        _enemies.Add(enemy);
    }

    public void RemoveEnemy(EnemyUnit enemy)
    {
        if (!_enemies.Remove(enemy))
            return;

        if (_enemies.Count > 0 || _spawningWave || _movePanel == null)
            return;

        TryRaiseDifficulty();
        SpawnNextWave();
    }

    public EnemyUnit FindNearestEnemy(Vector3 origin, float range)
    {
        float rangeSqr = range * range;
        EnemyUnit nearest = null;
        float nearestSqr = rangeSqr;
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            EnemyUnit enemy = _enemies[i];
            if (enemy == null || enemy.IsDead)
            {
                _enemies.RemoveAt(i);
                continue;
            }

            float sqr = (enemy.transform.position - origin).sqrMagnitude;
            if (sqr > nearestSqr)
                continue;

            nearestSqr = sqr;
            nearest = enemy;
        }

        return nearest;
    }

    private void OnValidate()
    {
        _enemyCount = Mathf.Max(0, _enemyCount);
        _difficulty = Mathf.Max(1, _difficulty);
    }

    public async Task<bool> BeginAsync()
    {
        if (_map != null)
            return true;

        var resources = GameManager.Instance.ResourceManager;
        if (resources == null)
        {
            Debug.LogError("[InGameManager] ResourceManager가 없습니다.");
            return false;
        }

        _map = await resources.InstantiateAsync(PublicVariable.MapPrefabAddress, null, false);
        if (_map == null)
            return false;

        _mapInfo = _map.GetComponent<MapInfo>();
        if (_mapInfo == null)
        {
            Debug.LogError("[InGameManager] MapInfo가 없습니다.");
            return false;
        }

        await SetupPlayer(resources);
        if (PlayerUnit == null)
            return false;

        SetupCamera();
        await RestoreCompanions(resources);
        PlayerDataManager playerData = GameManager.Instance != null ? GameManager.Instance.PlayerDataManager : null;
        if (playerData != null)
            _difficulty = Mathf.Max(1, playerData.Difficulty);

        await SpawnEnemies(resources);
        OpenHud();
        return true;
    }

    public async Task<bool> AddCompanionAsync()
    {
        if (_addingCompanion || PlayerUnit == null || _weaponTable == null || _playerPositions == null)
            return false;

        if (CompanionCount >= MaxCompanionCount)
            return false;

        PlayerDataManager playerData = GameManager.Instance != null ? GameManager.Instance.PlayerDataManager : null;
        int ownedCount = CompanionCount;
        if (playerData == null || playerData.Gold < PlayerDataManager.GetCompanionCost(ownedCount))
            return false;

        int index = _allies.Count;

        ResourceManager resources = GameManager.Instance != null ? GameManager.Instance.ResourceManager : null;
        if (resources == null)
            return false;

        _addingCompanion = true;
        try
        {
            AllyUnit ally = await SpawnAllyAsync(resources, index);
            if (ally == null)
                return false;

            if (!playerData.TryPayCompanion(ownedCount))
            {
                resources.ReleaseInstance(ally.gameObject);
                return false;
            }

            _allies.Add(ally);
            CameraManager cameraManager = GameManager.Instance != null ? GameManager.Instance.CameraManager : null;
            if (cameraManager != null)
                cameraManager.SetCompanionCount(CompanionCount);
            playerData.SetCompanionCount(CompanionCount);
            return true;
        }
        finally
        {
            _addingCompanion = false;
        }
    }

    private async Task RestoreCompanions(ResourceManager resources)
    {
        PlayerDataManager playerData = GameManager.Instance != null ? GameManager.Instance.PlayerDataManager : null;
        int savedCount = playerData != null ? playerData.CompanionCount : 0;
        int count = Mathf.Min(savedCount, MaxCompanionCount);
        for (int i = 0; i < count; i++)
        {
            AllyUnit ally = await SpawnAllyAsync(resources, _allies.Count);
            if (ally == null)
                break;

            _allies.Add(ally);
        }

        if (CompanionCount <= 0)
            return;

        CameraManager cameraManager = GameManager.Instance != null ? GameManager.Instance.CameraManager : null;
        if (cameraManager != null)
            cameraManager.SetCompanionCount(CompanionCount);
    }

    private void OpenHud()
    {
        if (PlayerUnit == null)
            return;

        UIManager ui = GameManager.Instance != null ? GameManager.Instance.UIManager : null;
        if (ui == null)
            return;

        InGameHud.Open(ui.UiRoot, PlayerUnit, this);
    }

    private async Task SetupPlayer(ResourceManager resources)
    {
        if (_mapInfo.PlayerPoint == null)
        {
            Debug.LogError("[InGameManager] 플레이어 위치가 없습니다.");
            return;
        }

        _playerPositions = await resources.LoadAsync<PlayerPosData>(PublicVariable.PlayerPosSoAddress);
        if (_playerPositions == null || _playerPositions.Count <= 0)
        {
            Debug.LogError("[InGameManager] PlayerPosData가 없습니다.");
            return;
        }

        _weaponTable = await resources.LoadAsync<WeaponSO>(PublicVariable.WeaponSoAddress);
        if (_weaponTable == null)
        {
            Debug.LogError("[InGameManager] WeaponSO가 없습니다.");
            return;
        }

        AllyUnit ally = await SpawnAllyAsync(resources, 0);
        if (ally == null)
            return;

        _allies.Add(ally);
    }

    private async Task<AllyUnit> SpawnAllyAsync(ResourceManager resources, int index)
    {
        if (!TryGetSlotPose(index, out Vector3 position, out Quaternion rotation))
            return null;

        GameObject instance = await resources.InstantiateAsync(
            PublicVariable.PlayerPrefabAddress,
            null,
            false,
            position);
        if (instance == null)
            return null;

        AllyUnit ally = instance.GetComponent<AllyUnit>();
        if (ally == null)
        {
            Debug.LogError("[InGameManager] AllyUnit이 없습니다.");
            resources.ReleaseInstance(instance);
            return null;
        }

        instance.transform.SetPositionAndRotation(position, rotation);
        await ally.EquipAsync(PublicVariable.DefaultWeaponTid, _weaponTable, resources);
        return ally;
    }

    private bool TryGetSlotPose(int index, out Vector3 position, out Quaternion rotation)
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;
        Transform point = _mapInfo != null ? _mapInfo.PlayerPoint : null;
        if (point == null || _playerPositions == null || index < 0 || index >= _playerPositions.Count)
            return false;

        Vector3 local = _playerPositions.Get(index);
        position = point.position + point.right * local.x + point.up * local.y + point.forward * local.z;
        rotation = point.rotation;
        return true;
    }

    private void SetupCamera()
    {
        var point = _mapInfo.CameraPoint;
        if (point == null)
        {
            Debug.LogError("[InGameManager] 카메라 위치가 없습니다.");
            return;
        }

        CameraManager cameraManager = GameManager.Instance != null ? GameManager.Instance.CameraManager : null;
        if (cameraManager == null)
        {
            Debug.LogError("[InGameManager] CameraManager가 없습니다.");
            return;
        }

        cameraManager.Setup(point);
    }

    private async Task SpawnEnemies(ResourceManager resources)
    {
        if (_spawningWave || Player == null || _mapInfo == null || resources == null)
            return;

        _spawningWave = true;
        _waveSpawnTime = -1f;
        try
        {
            if (!_hasWavePath)
            {
                _spawnPosition = _mapInfo.GetSpawnPosition(Player.transform);
                _stopPosition = _mapInfo.GetStopPosition(Player.transform);
                _spawnRotation = Player.transform.rotation;
                _hasWavePath = true;
            }

            if (_movePanel == null)
            {
                GameObject panelObject = await resources.InstantiateAsync(
                    PublicVariable.MovePanelPrefabAddress,
                    null,
                    false,
                    _spawnPosition);
                if (panelObject == null)
                    return;

                panelObject.transform.SetPositionAndRotation(_spawnPosition, _spawnRotation);
                _movePanel = panelObject.GetComponent<MovePanel>();
                if (_movePanel == null)
                {
                    Debug.LogError("[InGameManager] MovePanel이 없습니다.");
                    return;
                }
            }
            else
            {
                _movePanel.transform.SetPositionAndRotation(_spawnPosition, _spawnRotation);
            }

            if (_enemyTable == null)
            {
                _enemyTable = await resources.LoadAsync<EnemyDifficultySO>(PublicVariable.EnemySoAddress);
                if (_enemyTable == null)
                {
                    Debug.LogError("[InGameManager] EnemyDifficultySO가 없습니다.");
                    return;
                }
            }

            _difficulty = FindDifficulty(_difficulty);
            if (!_enemyTable.TryGet(_difficulty, out EnemyDifficultyData difficulty)
                || difficulty == null
                || difficulty.Enemies == null
                || difficulty.Enemies.Length == 0)
            {
                Debug.LogError($"[InGameManager] 난이도 {_difficulty} 적 데이터가 없습니다.");
                return;
            }

            int count = Mathf.Max(0, _enemyCount);
            for (int i = 0; i < count; i++)
            {
                EnemyData data = difficulty.Enemies[i % difficulty.Enemies.Length];
                EnemyUnit enemyUnit = await GetEnemyAsync(resources, data.Preset);
                if (enemyUnit == null)
                    return;

                enemyUnit.ApplyData(data);
                AddEnemy(enemyUnit);
                _movePanel.AddUnit(enemyUnit.transform);
            }

            _movePanel.MoveTo(_stopPosition);
            _waveSpawnTime = Time.time;
        }
        finally
        {
            _spawningWave = false;
        }
    }

    private void TryRaiseDifficulty()
    {
        if (_waveSpawnTime < 0f)
            return;

        if (Time.time - _waveSpawnTime > PublicVariable.DifficultyClearSeconds)
            return;

        if (_enemyTable == null || !_enemyTable.TryGet(_difficulty + 1, out _))
            return;

        _difficulty++;
        _waveSpawnTime = -1f;
        PlayerDataManager playerData = GameManager.Instance != null ? GameManager.Instance.PlayerDataManager : null;
        if (playerData != null)
            playerData.SetDifficulty(_difficulty);
    }

    private int FindDifficulty(int difficulty)
    {
        if (_enemyTable == null)
            return Mathf.Max(1, difficulty);

        if (_enemyTable.TryGet(difficulty, out _))
            return difficulty;

        int best = 0;
        EnemyDifficultyData[] list = _enemyTable.Difficulties;
        if (list == null)
            return Mathf.Max(1, difficulty);

        for (int i = 0; i < list.Length; i++)
        {
            EnemyDifficultyData entry = list[i];
            if (entry == null || entry.Difficulty > difficulty || entry.Difficulty <= best)
                continue;

            best = entry.Difficulty;
        }

        return best > 0 ? best : Mathf.Max(1, difficulty);
    }

    private async void SpawnNextWave()
    {
        ResourceManager resources = GameManager.Instance != null ? GameManager.Instance.ResourceManager : null;
        if (resources == null)
            return;

        await SpawnEnemies(resources);
    }
}
