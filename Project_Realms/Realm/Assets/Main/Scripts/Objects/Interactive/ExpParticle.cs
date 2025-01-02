using UnityEngine;

public class ExpParticle : MonoBehaviour
{
    [SerializeField] private float expAmount = 10f;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float accelerationRate = 2f; // 플레이어에게 가까워질수록 가속

    // 둥둥 떠다니는 효과를 위한 새로운 변수들
    [SerializeField] private float floatAmplitude = 0.3f; // 위아래로 움직이는 범위
    [SerializeField] private float floatFrequency = 2f;   // 움직임의 주기
    private Vector3 startPosition;
    private float floatTimer;

    private Player player;
    private bool isAttracting = false;
    private float currentSpeed;

    private void Start()
    {
        player = GameManager.Instance.player;
        currentSpeed = moveSpeed;
        startPosition = transform.position;
        floatTimer = Random.Range(0f, 2f * Mathf.PI); // 랜덤한 시작 위치
    }

    private void Update()
    {
        if (player == null || player.CharacterStats == null) return;

        // 둥둥 떠다니는 효과 적용
        if (!isAttracting)
        {
            floatTimer += Time.deltaTime;
            float yOffset = Mathf.Sin(floatTimer * floatFrequency) * floatAmplitude;
            transform.position = startPosition + new Vector3(0f, yOffset, 0f);
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        float expRange = player.CharacterStats.GetStatValue(StatType.ExpRange);

        // expRange가 0이하일 경우 처리
        if (expRange <= 0f) expRange = 0.1f;

        // ExpRange 범위 안에 들어왔는지 체크
        if (distanceToPlayer <= expRange)
        {
            isAttracting = true;
        }

        if (isAttracting)
        {
            // 속도 계산 시 안전장치 추가
            float speedMultiplier = Mathf.Clamp01(1 - distanceToPlayer / expRange);
            currentSpeed = moveSpeed + (moveSpeed * accelerationRate * speedMultiplier);

            Vector3 directionVector = player.transform.position - transform.position;
            // 방향 벡터가 0인 경우 체크
            if (directionVector.sqrMagnitude > 0.001f)
            {
                Vector3 direction = directionVector.normalized;
                Vector3 newPosition = transform.position + direction * currentSpeed * Time.deltaTime;

                // 위치 값 유효성 검사
                if (!float.IsInfinity(newPosition.x) && !float.IsInfinity(newPosition.y) && !float.IsInfinity(newPosition.z))
                {
                    transform.position = newPosition;
                }
                else
                {
                    Debug.LogWarning($"Invalid position detected. Current: {transform.position}, Target: {player.transform.position}");
                    isAttracting = false;
                }
            }

            if (distanceToPlayer < 0.5f)
            {
                player.GainExperience(expAmount);
                PoolManager.Instance.Despawn<ExpParticle>(this);
            }
        }

        if (distanceToPlayer > 15f)
        {
            player.GainExperience(expAmount);
            PoolManager.Instance.Despawn<ExpParticle>(this);
        }
    }

    // 경험치 양 설정 메서드
    public void SetExpAmount(float amount)
    {
        expAmount = amount;
    }

#if UNITY_EDITOR
    // 시각적 디버깅을 위한 기즈모
    private void OnDrawGizmos()
    {
        if (player != null && Application.isPlaying)
        {
            float expRange = player.CharacterStats.GetStatValue(StatType.ExpRange);
            Gizmos.color = isAttracting ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
#endif
}