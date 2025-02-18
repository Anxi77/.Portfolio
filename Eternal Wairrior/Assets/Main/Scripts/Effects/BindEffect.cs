using UnityEngine;

public class BindEffect : MonoBehaviour, IPoolable
{
    private ParticleSystem[] particleSystems;

    private void Awake()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>(true);
    }

    public void OnSpawnFromPool()
    {
        foreach (var ps in particleSystems)
        {
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Play(true);
            }
        }

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public void OnReturnToPool()
    {
        foreach (var ps in particleSystems)
        {
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        transform.SetParent(null);
    }
}