using Fusion;
using UnityEngine;

public class AsteroidController : NetworkBehaviour
{
    [SerializeField] private int _points = 3;
    [HideInInspector][Networked] public NetworkBool IsBig { get; set; }
    [HideInInspector][Networked] public NetworkBool IsMedium { get; set; }
    [HideInInspector][Networked] public NetworkBool IsSmall { get; set; }
    private TickTimer _despawnTimer;
    private bool _wasHit;
    private NetworkTransform _networkTransform;
    public bool IsAlive => !_wasHit;
    private ScreenBounds bounds;

    public override void Spawned()
    {
        bounds = FindObjectOfType<ScreenBounds>();
        _networkTransform = GetComponent<NetworkTransform>();
        _networkTransform.InterpolationTarget.localScale = Vector3.one;

        transform.Rotate(new Vector3(0, 0, Random.Range(0, 360)), Space.Self);
    }

    public void HitAsteroid(PlayerRef player)
    {
        _wasHit = true;
        _despawnTimer = TickTimer.CreateFromSeconds(Runner, .2f);

        if (Object == null || (player != PlayerRef.None && player != Runner.LocalPlayer)) return;

        if (Runner.TryGetPlayerObject(player, out var playerNetworkObject))
        {
            playerNetworkObject.GetComponent<PlayerNetworkData>().AddToScore(_points);
        }

        RPC_HitAsteroid();
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority && _wasHit && _despawnTimer.Expired(Runner))
        {
            _wasHit = false;
            if (Object.HasStateAuthority)
            {
                if (IsBig)
                {
                    FindObjectOfType<AsteroidSpawner>().BreakUpBigAsteroid(transform.position);
                }

                if (IsMedium)
                {
                    FindObjectOfType<AsteroidSpawner>().BreakUpMediumAsteroid(transform.position);
                }

                Runner.Despawn(Object);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        if (bounds.AmIOutOfBounds(transform.position))
        {
            Vector2 newPosition = bounds.CalculateWrappedPosition(transform.position);
            GetComponent<NetworkTransform>().TeleportToPosition(newPosition);
        }
    }

    public override void Render()
    {
        if (_wasHit && _despawnTimer.IsRunning)
        {
            _networkTransform.InterpolationTarget.localScale *= .95f;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_HitAsteroid()
    {
        _wasHit = true;
        _despawnTimer = TickTimer.CreateFromSeconds(Runner, .2f);
    }
}
