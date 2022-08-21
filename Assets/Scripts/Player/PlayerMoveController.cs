using Fusion;
using UnityEngine;

public class PlayerMoveController : NetworkBehaviour
{
    [SerializeField] private float _maxSpeed = 100.0f;
    [SerializeField] private float forwardSpeed = 30;
    [SerializeField] private float rotateSpeed = 15;
    private ScreenBounds bounds;
    private Rigidbody2D _rigidbody = null;
    private PlayerController _playerController = null;
    private PlayerVisualController _playerVisualController = null;

    public override void Spawned()
    {
        _playerController = GetComponent<PlayerController>();
        _playerVisualController = GetComponent<PlayerVisualController>();
        bounds = FindObjectOfType<ScreenBounds>();

        if (Object.HasStateAuthority == false) return;
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority == false) return;
        if (_playerController.AcceptInput == false) return;
        if (Runner.TryGetInputForPlayer<PlayerShipInput>(Object.InputAuthority, out var input))
        {
            Move(input);
        }

        if (bounds.AmIOutOfBounds(transform.position))
        {
            Vector2 newPosition = bounds.CalculateWrappedPosition(transform.position);
            GetComponent<NetworkTransform>().TeleportToPosition(newPosition);
        }
    }

    private void Move(PlayerShipInput input)
    {
        if (input.clockwise && !input.accelerate) //D
            _rigidbody.AddTorque(-rotateSpeed);

        if (input.anticlockwise && !input.accelerate) //A
            _rigidbody.AddTorque(rotateSpeed);

        if (!input.accelerate)
        {
            _playerVisualController.Accelerated(false);
            return;
        }

        _playerVisualController.Accelerated(true);

        _rigidbody.AddForce(transform.up.normalized * forwardSpeed);

        if (_rigidbody.velocity.magnitude > _maxSpeed) {
            _rigidbody.velocity = _rigidbody.velocity.normalized * _maxSpeed;
        }

        if (input.rotateForward) //W
            _rigidbody.AddTorque(-rotateSpeed);

        else if (input.rotateBackward) //S
            _rigidbody.AddTorque(rotateSpeed);

        
    }
}