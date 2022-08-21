using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private NetworkRunner _networkRunnerPrefab = null;
    [SerializeField] private PlayerData _playerDataPrefab = null;
    [SerializeField] private InputField _roomName = null;
    [SerializeField] private Text _playerName = null;

    private NetworkRunner _runnerInstance = null;

    [SerializeField] private string _gameSceneName = null;

    string nameChoosen = string.Empty;

    private void Start()
    {
        SetPlayerData();
    }

    public void StartSharedSession()
    {
        StartGame(GameMode.Shared, _roomName.text, _gameSceneName);
    }

    private void SetPlayerData()
    {
        var playerData = FindObjectOfType<PlayerData>();
        if (playerData == null)
        {
            playerData = Instantiate(_playerDataPrefab);
        }

        playerData.SetNickName(PlayerData.GetRandomNickName());
        _playerName.text = playerData.GetNickName();
    }

    private async void StartGame(GameMode mode, string roomName, string sceneName)
    {
        _runnerInstance = FindObjectOfType<NetworkRunner>();
        if (_runnerInstance == null)
        {
            _runnerInstance = Instantiate(_networkRunnerPrefab);
        }

        _runnerInstance.ProvideInput = true;

        var startGameArgs = new StartGameArgs()
        {
            GameMode = mode,
            SessionName = roomName,
            ObjectPool = _runnerInstance.GetComponent<NetworkObjectPoolDefault>(),
        };

        await _runnerInstance.StartGame(startGameArgs);

        _runnerInstance.SetActiveScene(sceneName);
    }
}
