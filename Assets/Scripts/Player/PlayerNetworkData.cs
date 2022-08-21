using Fusion;
using UnityEngine;

public class PlayerNetworkData : NetworkBehaviour
{
    private const int STARTING_LIVES = 3;
    private PlayerOverviewPanel _overviewPanel = null;

    [HideInInspector]
    [Networked(OnChanged = nameof(OnNickNameChanged))]
    public NetworkString<_16> NickName { get; private set; }

    [HideInInspector]
    [Networked(OnChanged = nameof(OnLivesChanged))]
    public int Lives { get; private set; }

    [HideInInspector]
    [Networked(OnChanged = nameof(OnScoreChanged))]
    public int Score { get; private set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            var nickName = FindObjectOfType<PlayerData>().GetNickName();
            NickName = nickName;
            Lives = STARTING_LIVES;
            Score = 0;
        }

        _overviewPanel = FindObjectOfType<PlayerOverviewPanel>();
        _overviewPanel.AddEntry(Object.InputAuthority, this);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        _overviewPanel.RemoveEntry(Object.InputAuthority);
    }

    public void AddToScore(int points)
    {
        Score += points;
    }

    public void SubtractLife()
    {
        Lives--;
    }

    public static void OnNickNameChanged(Changed<PlayerNetworkData> playerInfo)
    {
        playerInfo.Behaviour._overviewPanel.UpdateNickName(playerInfo.Behaviour.Object.InputAuthority,
            playerInfo.Behaviour.NickName.ToString());
        
    }

    public static void OnScoreChanged(Changed<PlayerNetworkData> playerInfo)
    {
        playerInfo.Behaviour._overviewPanel.UpdateScore(playerInfo.Behaviour.Object.InputAuthority,
            playerInfo.Behaviour.Score);
    }

    public static void OnLivesChanged(Changed<PlayerNetworkData> playerInfo)
    {
        playerInfo.Behaviour._overviewPanel.UpdateLives(playerInfo.Behaviour.Object.InputAuthority,
            playerInfo.Behaviour.Lives);
    }
}