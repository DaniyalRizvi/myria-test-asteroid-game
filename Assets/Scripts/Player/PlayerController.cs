using Fusion;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float _respawnDelay = 4.0f;
    [SerializeField] private float _spaceshipDamageRadius = 2.5f;
    private Rigidbody2D _rigidbody = null;
    private PlayerNetworkData _playerDataNetworked = null;
    private PlayerVisualController _visualController = null;
    public bool AcceptInput => _isAlive && Object.IsValid;

    private bool _collisionImmunity = false;
    [SerializeField] private float _immunityInSeconds = 3.0f;

    [Networked(OnChanged = nameof(OnAliveStateChanged))]
    private NetworkBool _isAlive { get; set; }
    private bool hitAsteroid = false;

    [Networked] private TickTimer _respawnTimer { get; set; }

    private GameStateController _gameStateController;

    public override void Spawned()
    {
        _visualController = GetComponent<PlayerVisualController>();
        if (Object.HasStateAuthority == false) return;
        _rigidbody = GetComponent<Rigidbody2D>();
        _playerDataNetworked = GetComponent<PlayerNetworkData>();
        _isAlive = true;
        hitAsteroid = false;
        _gameStateController = FindObjectOfType<GameStateController>();
    }

    private static void OnAliveStateChanged(Changed<PlayerController> playerController)
    {
        playerController.LoadOld();
        var wasAlive = playerController.Behaviour._isAlive;

        playerController.LoadNew();
        var isAlive = playerController.Behaviour._isAlive;

        playerController.Behaviour.ToggleVisuals(wasAlive, isAlive);
    }

    private void ToggleVisuals(bool wasAlive, bool isAlive)
    {
        if (wasAlive == false && isAlive == true)
        {
            _visualController.TriggerSpawn();
        }
        else if (wasAlive == true && isAlive == false)
        {
            _visualController.TriggerDestruction();
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority == false) return;
        if (_respawnTimer.Expired(Runner) && _gameStateController.GameIsRunning)
        {
            _isAlive = true;
            _respawnTimer = default;
        }

        if(_isAlive && hitAsteroid)
        {
            ShipWasHit();
        }
    }

    private void ShipWasHit()
    {
        _isAlive = false;

        ResetShip();

        if (_playerDataNetworked.Lives > 1)
        {
            _respawnTimer = TickTimer.CreateFromSeconds(Runner, _respawnDelay);
        }
        else
        {
            _respawnTimer = default;
        }

        _playerDataNetworked.SubtractLife();
        FindObjectOfType<GameStateController>().RPC_CheckIfGameHasEnded();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("TagAsteroid") && !_collisionImmunity)
        {
            var asteroidController = collision.GetComponent<AsteroidController>();
            asteroidController.HitAsteroid(PlayerRef.None);
            hitAsteroid = true;
        }
    }

    private void ResetShip()
    {
        _collisionImmunity = true;
        Invoke(nameof(CancelImmunity), _immunityInSeconds);

        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = 0f;
        hitAsteroid = false;
    }

    private void CancelImmunity()
    {
        _collisionImmunity = false;
    }

    public bool CheckImmunity()
    {
        return _collisionImmunity;
    }
}