using System.Threading.Tasks;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private ResourceManager _resourceManager;
    private SoundManager _soundManager;
    private UIManager _uiManager;
    private CameraManager _cameraManager;
    private PlayerDataManager _playerDataManager;
    private InGameManager _inGameManager;
    private bool _startRequested;

    public ResourceManager ResourceManager
    {
        get
        {
            ManagerBase.EnsureManager(transform, ref _resourceManager);
            return _resourceManager;
        }
    }

    public SoundManager SoundManager
    {
        get
        {
            ManagerBase.EnsureManager(transform, ref _soundManager);
            return _soundManager;
        }
    }

    public UIManager UIManager
    {
        get
        {
            ManagerBase.EnsureManager(transform, ref _uiManager);
            return _uiManager;
        }
    }

    public CameraManager CameraManager
    {
        get
        {
            ManagerBase.EnsureManager(transform, ref _cameraManager);
            return _cameraManager;
        }
    }

    public PlayerDataManager PlayerDataManager
    {
        get
        {
            ManagerBase.EnsureManager(transform, ref _playerDataManager);
            return _playerDataManager;
        }
    }

    public InGameManager InGameManager
    {
        get
        {
            ManagerBase.EnsureManager(transform, ref _inGameManager);
            return _inGameManager;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        ApplyMobileRuntime();
        StartGameAsync();
    }

    private async void StartGameAsync()
    {
        await PreloadBootAsync();
        PlayerDataManager playerData = PlayerDataManager;
        if (playerData == null)
        {
            Debug.LogError("[GameManager] PlayerDataManager가 없습니다.");
            return;
        }

        playerData.Load();

        UIManager ui = UIManager;
        if (ui == null)
        {
            Debug.LogError("[GameManager] UIManager가 없습니다.");
            return;
        }

        ui.ShowStart(RequestStart);
    }

    public void RequestStart()
    {
        if (_startRequested)
            return;

        _startRequested = true;
        StartInGameAsync();
    }

    private async void StartInGameAsync()
    {
        InGameManager inGameManager = InGameManager;
        if (inGameManager == null)
        {
            Debug.LogError("[GameManager] InGameManager가 없습니다.");
            _startRequested = false;
            return;
        }

        bool started = await inGameManager.BeginAsync();
        if (!started)
        {
            _startRequested = false;
            return;
        }

        UIManager ui = UIManager;
        if (ui != null)
            ui.Close();

        HideBootCover();
    }

    private static void ApplyMobileRuntime()
    {
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.orientation = ScreenOrientation.Portrait;
        Application.targetFrameRate = PublicVariable.TargetFrameRate;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    public async Task PreloadBootAsync()
    {
        var resourceManager = ResourceManager;
        if (resourceManager == null)
        {
            Debug.LogError("[GameManager] ResourceManager가 없습니다.");
            return;
        }

        await resourceManager.PreloadLabelAsync(PublicVariable.BootLabel);
    }

    private static void HideBootCover()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];
            if (canvas == null || canvas.gameObject.name != "Canvas")
                continue;

            canvas.gameObject.SetActive(false);
        }
    }
}
