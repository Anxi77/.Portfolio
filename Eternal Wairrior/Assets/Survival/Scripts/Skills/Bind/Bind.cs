using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Pool;

public class Bind : MonoBehaviour
{
    public float damage;
    public float duration;
    public float cooldown;

    public GameObject bindPrefab;

    private List<GameObject> spawnedBindEffects = new List<GameObject>();

    private void Start()
    {
        StartCoroutine(Binding());
    }

    private IEnumerator Binding()
    {
        while (true)
        {
            yield return new WaitForSeconds(cooldown);

            List<Enemy> affectedEnemies = new List<Enemy>();

            if (GameManager.Instance.enemies != null)
            {
                foreach (Enemy enemy in GameManager.Instance.enemies)
                {
                    if (enemy != null)
                    {
                        affectedEnemies.Add(enemy);
                        enemy.moveSpeed = 0;
                        GameObject spawnedEffect = LeanPool.Spawn(bindPrefab, enemy.transform);
                        spawnedBindEffects.Add(spawnedEffect);
                    }
                }
            }

            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                foreach (Enemy enemy in affectedEnemies)
                {
                    if (enemy != null)
                    {
                        enemy.TakeDamage(damage);
                    }
                }
                yield return new WaitForSeconds(0.1f);
                elapsedTime += 0.1f;
            }

            // duration ���� �� �ӵ� ���� �� ����Ʈ ����
            foreach (Enemy enemy in affectedEnemies)
            {
                if (enemy != null)
                {
                    enemy.moveSpeed = enemy.originalMoveSpeed;
                }
            }

            // ������ ����Ʈ ����
            foreach (GameObject effect in spawnedBindEffects)
            {
                if (effect != null)
                {
                    LeanPool.Despawn(effect);
                }
            }
            spawnedBindEffects.Clear();
        }
    }
}
