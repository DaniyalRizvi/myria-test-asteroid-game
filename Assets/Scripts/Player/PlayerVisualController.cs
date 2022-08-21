using Fusion;
using UnityEngine;

public class PlayerVisualController : SimulationBehaviour, ISpawned
{
    [SerializeField] private SpriteRenderer _spaceshipModel = null;
    [SerializeField] private ParticleSystem _destructionVFX = null;
    [SerializeField] private ParticleSystem _engineTrailVFX = null;

    public void Spawned()
    {
        var playerRef = Object.InputAuthority;
        _spaceshipModel.color = GetColor(playerRef);
    }

    public void TriggerSpawn()
    {
        _spaceshipModel.enabled = true;
        _destructionVFX.Stop();
    }

    public void Accelerated(bool flag)
    {
        if(flag)
            _engineTrailVFX.Play();

        else
            _engineTrailVFX.Stop();
    }

    public void TriggerDestruction()
    {
        _spaceshipModel.enabled = false;
        _engineTrailVFX.Stop();
        _destructionVFX.Play();
    }

    public static Color GetColor(int player)
    {
        switch (player)
        {
            case 0: return Color.red;
            case 1: return Color.green;
            case 2: return Color.blue;
            case 3: return Color.yellow;
            case 4: return Color.cyan;
            case 5: return Color.grey;
            case 6: return Color.magenta;
            case 7: return Color.white;
        }

        return Color.black;
    }
}
