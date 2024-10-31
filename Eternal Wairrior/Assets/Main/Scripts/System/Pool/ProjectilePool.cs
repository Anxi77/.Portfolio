using UnityEngine;
using System.Collections.Generic;

public class ProjectilePool : SingletonManager<ProjectilePool>
{
    private Dictionary<string, Queue<Projectile>> pools = new Dictionary<string, Queue<Projectile>>();
    private Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();
    private Transform poolContainer;

    protected override void Awake()
    {
        base.Awake();
        poolContainer = new GameObject("ProjectilePool").transform;
        poolContainer.parent = transform;
    }

    public Projectile SpawnProjectile(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        string prefabId = prefab.name;

        // �ش� �������� Ǯ�� ���ٸ� ���� ����
        if (!pools.ContainsKey(prefabId))
        {
            pools[prefabId] = new Queue<Projectile>();
            prefabDictionary[prefabId] = prefab;
        }

        Projectile projectile;

        // Ǯ�� ���� ������ �߻�ü�� �ִ��� Ȯ��
        if (pools[prefabId].Count > 0)
        {
            projectile = pools[prefabId].Dequeue();
            projectile.transform.position = position;
            projectile.transform.rotation = rotation;
            projectile.gameObject.SetActive(true);
        }
        else
        {
            // Ǯ�� ��������� ���� ����
            GameObject newObj = Instantiate(prefab, position, rotation, poolContainer);
            projectile = newObj.GetComponent<Projectile>();
        }

        return projectile;
    }

    public void DespawnProjectile(Projectile projectile)
    {
        string prefabId = projectile.gameObject.name.Replace("(Clone)", "");

        projectile.gameObject.SetActive(false);

        // Ǯ�� ��ȯ
        if (!pools.ContainsKey(prefabId))
        {
            pools[prefabId] = new Queue<Projectile>();
        }

        pools[prefabId].Enqueue(projectile);
    }
}