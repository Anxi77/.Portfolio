using UnityEngine;
using System.Collections;
using Assets.FantasyMonsters.Common.Scripts;

public class BossMonster : Enemy
{
    [Header("Boss Specific Stats")]
    public float enrageThreshold = 0.3f; // ü�� 30% ������ �� �ݳ�
    public float enrageDamageMultiplier = 1.5f;
    public float enrageSpeedMultiplier = 1.3f;

    public Monster monster;

    private bool isEnraged = false;
    private Vector3 startPosition;
    private Animator animator;  

    protected override void Start()
    {
        base.Start();
        startPosition = transform.position;
        animator = GetComponentInChildren<Animator>();
        InitializeBossStats();
    }

    private void InitializeBossStats()
    {
        // ���� �⺻ ���� ����
        hp *= 5f;  // �Ϲ� ���ͺ��� 5���� ü��
        damage *= 2f;  // 2���� ������
        moveSpeed *= 0.8f;  // 80%�� �̵��ӵ�
        baseDefense *= 2f;  // 2���� ����
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        // ü���� Ư�� ���� ���Ϸ� �������� �ݳ� ����
        if (!isEnraged && hp <= maxHp * enrageThreshold)
        {
            EnterEnragedState();
        }
    }

    public override void Move()
    {
        base.Move();
        monster.SetState(MonsterState.Run);
    }

    private void EnterEnragedState()
    {
        isEnraged = true;
        damage *= enrageDamageMultiplier;
        moveSpeed *= enrageSpeedMultiplier;

        // �ݳ� ����Ʈ ���
        PlayEnrageEffect();
    }

    private void PlayEnrageEffect()
    {
        // �ݳ� ���� ����Ʈ ��� ����
        // ��ƼŬ �ý��� ���� ���
    }

    public override void Die()
    {
        MonsterManager.Instance.OnBossDefeated(transform.position);
        base.Die();
    }

    // ���� ���� ���� ���ϵ�
    //private IEnumerator SpecialAttackPattern()
    //{
    //    while (true)
    //    {
    //        // �⺻ ����
    //        yield return new WaitForSeconds(3f);

    //        // ���� ����
    //        if (hp < maxHp * 0.7f)
    //        {
    //            AreaAttack();
    //            yield return new WaitForSeconds(5f);
    //        }

    //        // ��ȯ ����
    //        if (hp < maxHp * 0.5f)
    //        {
    //            SummonMinions();
    //            yield return new WaitForSeconds(10f);
    //        }
    //    }
    //}

    //private void AreaAttack()
    //{
    //    // ���� ���� ����
    //}

    //private void SummonMinions()
    //{
    //    // �ϼ��� ��ȯ ����
    //}
}