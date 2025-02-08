using UnityEngine;

public class BindEffect : MonoBehaviour, IPoolable
{
    private ParticleSystem[] particleSystems;

    private void Awake()
    {
        // 모든 파티클 시스템을 한 번에 가져옴 (자신과 자식들의 파티클 시스템)
        particleSystems = GetComponentsInChildren<ParticleSystem>(true);
    }

    public void OnSpawnFromPool()
    {
        Debug.Log($"BindEffect spawned: {gameObject.name}, Particle count: {particleSystems.Length}");

        // 모든 파티클 시스템 재생
        foreach (var ps in particleSystems)
        {
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Play(true);  // withChildren = true로 설정
            }
        }

        // Transform 초기화
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public void OnReturnToPool()
    {
        // 모든 파티클 시스템 정지
        foreach (var ps in particleSystems)
        {
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        transform.SetParent(null);
    }

    // 디버그용 메서드
    private void OnEnable()
    {
        Debug.Log($"BindEffect enabled: {gameObject.name}");
    }
}