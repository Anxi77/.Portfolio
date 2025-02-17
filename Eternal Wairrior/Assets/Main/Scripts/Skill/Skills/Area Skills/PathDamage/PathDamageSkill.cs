using UnityEngine;
using System.Collections.Generic;

public class PathDamageSkill : AreaSkills
{
    [SerializeField] private float _pathWidth = 2f;
    [SerializeField] private float _minDistanceBetweenPoints = 0.5f;
    [SerializeField] private DamageZone _damageZonePrefab;
    private List<Vector2> _pathPoints = new List<Vector2>();
    private List<GameObject> _activeZones = new List<GameObject>();
    private Vector2 _lastRecordedPosition;
    private bool _isActive = true;
    public override void Initialize()
    {
        base.Initialize();
        _lastRecordedPosition = transform.position;
    }

    public void StartPathDamage()
    {
        _isActive = true;
        _pathPoints.Clear();
        _pathPoints.Add(transform.position);
        _lastRecordedPosition = transform.position;

        foreach (var zone in _activeZones)
        {
            if (zone != null)
                PoolManager.Instance.Despawn<DamageZone>(zone.GetComponent<DamageZone>());
        }
        _activeZones.Clear();
    }

    public void StopPathDamage()
    {
        _isActive = false;
    }

    private void Update()
    {
        if (!_isActive) return;

        float distanceFromLast = Vector2.Distance((Vector2)transform.position, _lastRecordedPosition);

        if (distanceFromLast >= _minDistanceBetweenPoints)
        {
            _pathPoints.Add(transform.position);
            _lastRecordedPosition = transform.position;
            CreateDamageArea(transform.position);
        }
    }

    private void CreateDamageArea(Vector2 position)
    {
        DamageZone damageZone = PoolManager.Instance.Spawn<DamageZone>(_damageZonePrefab.gameObject, position, Quaternion.identity);

        if (damageZone != null)
        {
            damageZone.Initialize(Damage, Duration, TickRate, _pathWidth);

            CircleCollider2D collider = damageZone.GetComponent<CircleCollider2D>();
            if (collider != null)
            {
                collider.radius = 0.5f;
                collider.isTrigger = true;
            }

            _activeZones.Add(damageZone.gameObject);
            StartCoroutine(DespawnAfterDuration(damageZone.gameObject));
        }
        else
        {
            print("no DamageZone");
        }
    }

    private System.Collections.IEnumerator DespawnAfterDuration(GameObject obj)
    {
        yield return new WaitForSeconds(Duration);
        if (obj != null)
        {
            _activeZones.Remove(obj);
            PoolManager.Instance.Despawn<DamageZone>(obj.GetComponent<DamageZone>());
        }
    }

    public override string GetDetailedDescription()
    {
        string baseDesc = "Creates a damaging path behind the player";
        if (skillData?.GetCurrentTypeStat() != null)
        {
            baseDesc += $"\n\nCurrent Effects:" +
                       $"\nDamage: {Damage:F1}" +
                       $"\nPath Width: {_pathWidth:F1}" +
                       $"\nDuration: {Duration:F1}s" +
                       $"\nDamage Interval: {TickRate:F1}s";
        }
        return baseDesc;
    }
}