using System;
using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class GameStateController : NetworkBehaviour
{
    enum GameState
    {
        Starting,
        Running,
        Ending
    }

    [SerializeField] private bool doControlTesting = false;
    [SerializeField] private float _startDelay = 4.0f;
    [SerializeField] private float _endDelay = 4.0f;
    [SerializeField] private float _gameSessionLength = 180.0f;
    [SerializeField] private Text _startEndDisplay = null;
    [SerializeField] private Text _ingameTimerDisplay = null;
    [Networked] private TickTimer _timer { get; set; }
    [Networked] private GameState _gameState { get; set; }
    [Networked] private NetworkBehaviourId _winner { get; set; }
    public bool GameIsRunning => _gameState == GameState.Running;

    //TODO: Need to guarantee that the game only support 4 playrs
    [Networked, Capacity(4)] private NetworkLinkedList<NetworkBehaviourId> _playerDataNetworkedIds => default;

    public override void Spawned()
    {
        _startEndDisplay.gameObject.SetActive(true);
        _ingameTimerDisplay.gameObject.SetActive(false);

        if (Object.HasStateAuthority == false) return;
        if (_gameState != GameState.Starting)
        {
            foreach (var player in Runner.ActivePlayers)
            {
                if (Runner.TryGetPlayerObject(player, out var playerObject) == false) continue;
                RPC_TrackNewPlayer(playerObject.GetComponent<PlayerNetworkData>().Id);
            }
        }

        _gameState = GameState.Starting;
        _timer = TickTimer.CreateFromSeconds(Runner, _startDelay);
    }


    public override void FixedUpdateNetwork()
    {
        switch (_gameState)
        {
            case GameState.Starting:
                UpdateStartingDisplay();
                break;
            case GameState.Running:
                UpdateRunningDisplay();
                if (_timer.ExpiredOrNotRunning(Runner) && Object.HasStateAuthority)
                {
                    GameHasEnded();
                }

                break;
            case GameState.Ending:
                UpdateEndingDisplay();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateStartingDisplay()
    {
        _startEndDisplay.text = $"Game Starts In {Mathf.RoundToInt(_timer.RemainingTime(Runner) ?? 0)}";

        if (Object.HasStateAuthority == false) return;
        if (_timer.ExpiredOrNotRunning(Runner) == false) return;

        FindObjectOfType<PlayerSpawner>().StartSpaceshipSpawner(this);

        if(!doControlTesting)
            FindObjectOfType<AsteroidSpawner>().StartAsteroidSpawner();

        RPC_SpawnReadyPlayers();
        _gameState = GameState.Running;
        _timer = TickTimer.CreateFromSeconds(Runner, _gameSessionLength);
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    private void RPC_SpawnReadyPlayers()
    {
        FindObjectOfType<PlayerSpawner>().SpawnSpaceship(Runner.LocalPlayer);
    }

    private void UpdateRunningDisplay()
    {
        _startEndDisplay.gameObject.SetActive(false);
        _ingameTimerDisplay.gameObject.SetActive(true);
        _ingameTimerDisplay.text =
            $"{Mathf.RoundToInt(_timer.RemainingTime(Runner) ?? 0).ToString("000")} seconds left";
    }

    private void UpdateEndingDisplay()
    {
        if (Runner.TryFindBehaviour(_winner, out PlayerNetworkData playerData) == false) return;

        _startEndDisplay.gameObject.SetActive(true);
        _ingameTimerDisplay.gameObject.SetActive(false);
        _startEndDisplay.text =
            $"{playerData.NickName} won with {playerData.Score} points. Disconnecting in {Mathf.RoundToInt(_timer.RemainingTime(Runner) ?? 0)}";
        _startEndDisplay.color = PlayerVisualController.GetColor(playerData.Object.InputAuthority);

        if (_timer.ExpiredOrNotRunning(Runner) == false) return;

        Runner.Shutdown();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_CheckIfGameHasEnded()
    {
        CheckIfGameHasEnded();
    }

    public void CheckIfGameHasEnded()
    {
        if (Object.HasStateAuthority == false) return;

        int playersAlive = 0;

        for (int i = 0; i < _playerDataNetworkedIds.Count; i++)
        {
            if (Runner.TryFindBehaviour(_playerDataNetworkedIds[i],
                    out PlayerNetworkData playerDataNetworkedComponent) == false)
            {
                _playerDataNetworkedIds.Remove(_playerDataNetworkedIds[i]);
                i--;
                continue;
            }

            if (playerDataNetworkedComponent.Lives > 0) playersAlive++;
        }

        if (playersAlive > 1) return;

        foreach (var playerDataNetworkedId in _playerDataNetworkedIds)
        {
            if (Runner.TryFindBehaviour(playerDataNetworkedId,
                    out PlayerNetworkData playerDataNetworkedComponent) ==
                false) continue;

            if (playerDataNetworkedComponent.Lives > 0 == false) continue;

            _winner = playerDataNetworkedId;
        }

        GameHasEnded();
    }

    private void GameHasEnded()
    {
        _timer = TickTimer.CreateFromSeconds(Runner, _endDelay);
        _gameState = GameState.Ending;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TrackNewPlayer(NetworkBehaviourId playerDataNetworkedId)
    {
        _playerDataNetworkedIds.Add(playerDataNetworkedId);
    }
}