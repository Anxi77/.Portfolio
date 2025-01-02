using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PouBall : MonoBehaviour
{
    public float damage = 10f;  // 데미지량
    public float speed = 5f;  // 이동 속도
    public float lifeTime = 5f;  // 투사체 수명
    private Vector3 direction;  // 이동 방향

    private void Start()
    {
        Destroy(gameObject, lifeTime);  // 수명이 다하면 삭제
    }

    // 목표 위치를 설정하고 방향 계산
    public void SetTarget(Vector3 targetPosition)
    {
        direction = (targetPosition - transform.position).normalized;
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;  // 이동
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);  // 적에게 데미지
            }
            Destroy(gameObject);  // 투사체 제거
        }
    }




}
