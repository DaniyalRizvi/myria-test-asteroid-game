using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class AsteroidSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef _smallAsteroid = NetworkPrefabRef.Empty;
    [SerializeField] private NetworkPrefabRef _mediumAsteroid = NetworkPrefabRef.Empty;
    [SerializeField] private NetworkPrefabRef _bigAsteroid = NetworkPrefabRef.Empty;

    [SerializeField] private float _asteroidSpeed = 1300.0f;

    // The minimum and maximum amount of time between each big asteroid spawn.
    [SerializeField] private float _minSpawnDelay = 3.0f;
    [SerializeField] private float _maxSpawnDelay = 7.0f;

    // The minimum and maximum amount of medium asteroids a big asteroids spawns when it gets destroyed.
    [SerializeField] private int _minMediumAsteroidSplinters = 3;
    [SerializeField] private int _maxMediumAsteroidSplinters = 6;

    // The minimum and maximum amount of small asteroids a medium asteroids spawns when it gets destroyed.
    [SerializeField] private int _minSmallAsteroidSplinters = 3;
    [SerializeField] private int _maxSmallAsteroidSplinters = 6;

    [Networked] private TickTimer _spawnDelay { get; set; }
    private List<NetworkId> _asteroids = new List<NetworkId>();
    Transform[] asteroidSpawnPoints;

    [SerializeField]
    GameObject spawnPointParent = null;

    void Awake()
    {
        AssignSpawnPoints();
    }

    void AssignSpawnPoints()
    {
        List<Transform> spawnTransforms = new List<Transform>();
        for (int i = 0; i < spawnPointParent.transform.childCount; i++)
        {
            spawnTransforms.Add(spawnPointParent.transform.GetChild(i));
        }
        asteroidSpawnPoints = spawnTransforms.ToArray();
    }

    Vector3 GetRandomSpawnPosition()
    {
        return asteroidSpawnPoints[Random.Range(0, asteroidSpawnPoints.Length)].position;
    }

    public void StartAsteroidSpawner()
    {
        if (Object.HasStateAuthority == false) return;
        SetSpawnDelay();
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority == false) return;
        SpawnAsteroid();
    }

    private void SpawnAsteroid()
    {
        if (_spawnDelay.Expired(Runner) == false) return;
        Vector3 position = GetRandomSpawnPosition();
        var asteroid = Runner.Spawn(_bigAsteroid, position, Quaternion.identity, PlayerRef.None, onBeforeSpawned: SpinBigAsteroid);
        _asteroids.Add(asteroid.Id);
        SetSpawnDelay();
    }

    private void SetSpawnDelay()
    {
        var time = Random.Range(_minSpawnDelay, _maxSpawnDelay);
        _spawnDelay = TickTimer.CreateFromSeconds(Runner, time);
    }

    private void SetSizeBool(AsteroidController asteroid, bool big = false, bool medium = false, bool small = false)
    {
        asteroid.IsBig = big;
        asteroid.IsMedium = medium;
        asteroid.IsSmall = small;
    }

    private void SpinBigAsteroid(NetworkRunner runner, NetworkObject asteroidNetworkObject)
    {
        Vector3 force = -asteroidNetworkObject.transform.position.normalized * _asteroidSpeed;
        var rb = asteroidNetworkObject.GetComponent<Rigidbody2D>();
        rb.AddForce(force);
        rb.AddTorque((_asteroidSpeed/ 10));
        var asteroidBehaviour = asteroidNetworkObject.GetComponent<AsteroidController>();
        SetSizeBool(asteroidBehaviour, true);
    }

    private void SpinMediumAsteroid(NetworkRunner runner, NetworkObject asteroidNetworkObject)
    {
        Transform objectTransform = asteroidNetworkObject.transform;
        Vector3 newPos = new Vector3(objectTransform.position.x, objectTransform.position.y + Random.Range(-40, 40), objectTransform.position.z);
        objectTransform.position = newPos;
        Vector3 force = -asteroidNetworkObject.transform.position.normalized * (_asteroidSpeed*2);
        var rb = asteroidNetworkObject.GetComponent<Rigidbody2D>();
        rb.AddForce(force);
        rb.AddTorque((_asteroidSpeed*2)/10);
        var asteroidBehaviour = asteroidNetworkObject.GetComponent<AsteroidController>();
        SetSizeBool(asteroidBehaviour, false, true);
    }

    private void SpinSmallAsteroid(NetworkRunner runner, NetworkObject asteroidNetworkObject)
    {
        Transform objectTransform = asteroidNetworkObject.transform;
        Vector3 newPos = new Vector3(objectTransform.position.x, objectTransform.position.y + Random.Range(-60, 60), objectTransform.position.z);
        objectTransform.position = newPos;
        Vector3 force = -asteroidNetworkObject.transform.position.normalized * (_asteroidSpeed*3);
        var rb = asteroidNetworkObject.GetComponent<Rigidbody2D>();
        rb.AddForce(force);
        rb.AddTorque((_asteroidSpeed*3)/10);
        var asteroidBehaviour = asteroidNetworkObject.GetComponent<AsteroidController>();
        SetSizeBool(asteroidBehaviour, false, false, true);
    }

    public void BreakUpBigAsteroid(Vector3 position)
    {
        int splintersToSpawn = Random.Range(_minMediumAsteroidSplinters, _maxMediumAsteroidSplinters);

        for (int counter = 0; counter < splintersToSpawn; counter++)
        {
            var asteroid = Runner.Spawn(_mediumAsteroid, position, Quaternion.identity,
                PlayerRef.None,
                (networkRunner, asteroidNetworkObject) =>
                    SpinMediumAsteroid(networkRunner, asteroidNetworkObject));
            _asteroids.Add(asteroid.Id);
        }
    }

    public void BreakUpMediumAsteroid(Vector3 position)
    {
        int splintersToSpawn = Random.Range(_minSmallAsteroidSplinters, _maxSmallAsteroidSplinters);

        for (int counter = 0; counter < splintersToSpawn; counter++)
        {
            var asteroid = Runner.Spawn(_smallAsteroid, position , Quaternion.identity,
                PlayerRef.None,
                (networkRunner, asteroidNetworkObject) =>
                    SpinSmallAsteroid(networkRunner, asteroidNetworkObject));
            _asteroids.Add(asteroid.Id);
        }
    }
}