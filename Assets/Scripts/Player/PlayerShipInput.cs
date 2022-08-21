using Fusion;

enum PlayerShipButtons
{
    Fire,
    Accelerate,
    RotateForward,
    RotateBackward,
    RotateClockwise,
    RotateAntiClockwise
}

public struct PlayerShipInput : INetworkInput
{
    public bool accelerate;
    public bool rotateForward;
    public bool rotateBackward;
    public bool clockwise;
    public bool anticlockwise;
    public NetworkButtons Buttons;
}