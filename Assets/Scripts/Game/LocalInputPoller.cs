using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class LocalInputPoller : MonoBehaviour, INetworkRunnerCallbacks
{
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        PlayerShipInput localInput = new PlayerShipInput();

        if (Input.GetKey(KeyCode.Space))
            localInput.accelerate = Input.GetKey(KeyCode.Space);

        if (Input.GetKey(KeyCode.W))
            localInput.rotateForward = Input.GetKey(KeyCode.W);

        if (Input.GetKey(KeyCode.S))
            localInput.rotateBackward = Input.GetKey(KeyCode.S);

        if (Input.GetKey(KeyCode.A))
            localInput.anticlockwise = Input.GetKey(KeyCode.A);

        if (Input.GetKey(KeyCode.D))
            localInput.clockwise = Input.GetKey(KeyCode.D);

        if (Input.GetMouseButton(0))
            localInput.Buttons.Set(PlayerShipButtons.Fire, Input.GetMouseButton(0));

        input.Set(localInput);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
        byte[] token)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }
}