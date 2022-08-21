using Fusion;
using UnityEngine;

public class PlayerFireController : NetworkBehaviour
{
    [SerializeField] private float _delayBetweenShots = 0.2f;
    [SerializeField] private NetworkPrefabRef _bullet = NetworkPrefabRef.Empty;

    [SerializeField] private Transform bulletSpawnPoint=null;
    private PlayerController _playerController = null;
    [Networked] private NetworkButtons _buttonsPrevious { get; set; }
    [Networked] private TickTimer _shootCooldown { get; set; }

    public override void Spawned()
    {
        _playerController = GetComponent<PlayerController>();
        if (Object.HasStateAuthority == false) return;
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority == false) return;
        if (_playerController.AcceptInput == false) return;
        if (GetInput<PlayerShipInput>(out var input) == false) return;
        Fire(input);
    }

    private void Fire(PlayerShipInput input)
    {
        if (input.Buttons.WasPressed(_buttonsPrevious, PlayerShipButtons.Fire))
        {
            SpawnBullet();
        }

        _buttonsPrevious = input.Buttons;
    }

    private void SpawnBullet()
    {
        if (_shootCooldown.ExpiredOrNotRunning(Runner) == false) return;

        Runner.Spawn(_bullet, bulletSpawnPoint.position, bulletSpawnPoint.rotation, Object.InputAuthority);

        _shootCooldown = TickTimer.CreateFromSeconds(Runner, _delayBetweenShots);
    }
}
