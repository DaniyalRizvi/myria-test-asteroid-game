using Fusion;
using UnityEngine;

public class BulletController : NetworkBehaviour
{
    [SerializeField] private float _maxLifetime = 3.0f;
    [SerializeField] private float _speed = 10.0f;
    private TickTimer _currentLifetime { get; set; }
    private Rigidbody2D _rigidbody=null;
    private ScreenBounds bounds;

    public override void Spawned()
    {
        bounds = FindObjectOfType<ScreenBounds>();

        if (Object.HasStateAuthority == false) return;
        _rigidbody = GetComponent<Rigidbody2D>();
        _currentLifetime = TickTimer.CreateFromSeconds(Runner, _maxLifetime);
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority == false) return;
        _rigidbody.velocity = Vector3.ClampMagnitude(_rigidbody.velocity, _speed);
        _rigidbody.AddForce(transform.up.normalized * _speed,ForceMode2D.Impulse);

        if (bounds.AmIOutOfBounds(transform.position))
        {
            Vector2 newPosition = bounds.CalculateWrappedPosition(transform.position);
            GetComponent<NetworkTransform>().TeleportToPosition(newPosition);
        }

        CheckLifetime();
    }

    private void CheckLifetime()
    {
        if (_currentLifetime.Expired(Runner) == false)
        {
            return;
        }
        Runner.Despawn(Object);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("TagAsteroid"))
        {
            var hitAsteroid = collision.gameObject.GetComponent<AsteroidController>();

            if (hitAsteroid == null)
                return;

            hitAsteroid.HitAsteroid(Object.StateAuthority);
            Runner.Despawn(Object);
        }
    }
}