using UnityEngine;

public class Barrier : MonoBehaviour
{
    [SerializeField] private int checkpointId;  // 이 배리어가 속한 체크포인트 ID

    private void Start()
    {
        EnableBarrier();
    }

    public void DisableBarrier()
    {
        print("dsiable 배리어 호출");
        gameObject.SetActive(false);
    }

    public void EnableBarrier()
    {
        gameObject.SetActive(true);
    }

    public int CheckpointId => checkpointId;
}