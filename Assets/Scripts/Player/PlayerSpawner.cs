using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, ISpawned
{
    [SerializeField] private NetworkPrefabRef _spaceshipNetworkPrefab = NetworkPrefabRef.Empty;
    private GameStateController _gameStateController = null;
    private SpawnPoint[] _spawnPoints = null;

    public void Spawned()
    {
        _spawnPoints = FindObjectsOfType<SpawnPoint>();

        if (FindObjectOfType<GameStateController>().GameIsRunning)
            SpawnSpaceship(Runner.LocalPlayer);
    }

    public void StartSpaceshipSpawner(GameStateController gameStateController)
    {
        _gameStateController = gameStateController;
    }

    public void SpawnSpaceship(PlayerRef player)
    {
        // Modulo is used in case there are more players than spawn points.
        int index = player % _spawnPoints.Length;
        var spawnPosition = _spawnPoints[index].transform.position;

        var playerObject = Runner.Spawn(_spaceshipNetworkPrefab, spawnPosition, Quaternion.identity, player);
        Runner.SetPlayerObject(player, playerObject);
        if (!_gameStateController)
            _gameStateController = FindObjectOfType<GameStateController>();

        _gameStateController.RPC_TrackNewPlayer(playerObject.GetComponent<PlayerNetworkData>().Id);
    }
}