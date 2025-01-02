using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraObstacleController : MonoBehaviour, IInitializable
{
    public Transform playerTarget;

    [SerializeField] private float fadeSpeed = 2f; // 페이드 속도 조절
    [SerializeField] private float rayDistance = 20f; // 레이 거리
    [SerializeField, Range(0f, 1f)] private float fadeAlphas = 0f;
    [SerializeField] private LayerMask obstacleLayer; // Inspector에서 장애물 레이어 설정
    [SerializeField] private float alphaThreshold = 0.01f; // 알파값 변경 임계값
    [SerializeField] private Transform[] rayPositions;

    private Dictionary<Renderer, float> originalAlphas = new Dictionary<Renderer, float>();
    private List<Renderer> currentlyTransparentObjects = new List<Renderer>();
    private bool isInitialized = false;
    public bool IsInitialized => isInitialized;
    private Camera mainCamera;

    public void Initialize()
    {
        isInitialized = true;
        mainCamera = GetComponent<Camera>();
    }

    public void Update()
    {
       
    }

    private void RayCastObstacleCleaner()
    {
        RestoreTransparency();

        foreach (Transform rayPos in rayPositions)
        {
            Vector3 directionToPlayer = (playerTarget.transform.position - rayPos.position).normalized;
            Ray ray = new Ray(rayPos.position, directionToPlayer);
            ProcessRaycast(ray);
        }
    }

    private void ProcessRaycast(Ray ray)
    {
        RaycastHit[] hits = Physics.RaycastAll(ray, rayDistance);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == playerTarget.gameObject)
            {
                break;
            }

            if (hit.collider.TryGetComponent<Renderer>(out Renderer obstacleRenderer))
            {
                if (!originalAlphas.ContainsKey(obstacleRenderer))
                {
                    originalAlphas[obstacleRenderer] = obstacleRenderer.material.color.a;
                }

                Color obstacleColor = obstacleRenderer.material.color;
                if (Mathf.Abs(obstacleColor.a - fadeAlphas) > alphaThreshold)
                {
                    obstacleColor.a = Mathf.Lerp(obstacleColor.a, fadeAlphas, Time.deltaTime * fadeSpeed);
                    obstacleRenderer.material.SetColor("_Color", obstacleColor);
                }

                if (!currentlyTransparentObjects.Contains(obstacleRenderer))
                {
                    currentlyTransparentObjects.Add(obstacleRenderer);
                }
            }
        }
    }

    private void RestoreTransparency()
    {
        for (int i = currentlyTransparentObjects.Count - 1; i >= 0; i--)
        {
            Renderer renderer = currentlyTransparentObjects[i];
            if (renderer != null)
            {
                Color color = renderer.material.color;
                float originalAlpha = originalAlphas[renderer];
                if (Mathf.Abs(color.a - originalAlpha) > alphaThreshold)
                {
                    color.a = Mathf.Lerp(color.a, originalAlpha, Time.deltaTime * fadeSpeed);
                    renderer.material.SetColor("_Color", color);
                }
                else
                {
                    color.a = originalAlpha;
                    renderer.material.SetColor("_Color", color);
                    currentlyTransparentObjects.RemoveAt(i);
                }
            }
            else
            {
                currentlyTransparentObjects.RemoveAt(i);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var kvp in originalAlphas)
        {
            if (kvp.Key != null)
            {
                Color color = kvp.Key.material.color;
                color.a = kvp.Value;
                kvp.Key.material.SetColor("_Color", color);
            }
        }
    }
    private void OnDrawGizmos()
    {
        if (playerTarget != null) 
        { 
            foreach (Transform rayPos in rayPositions)
            {
                Vector3 directionToPlayer = (playerTarget.transform.position - rayPos.position).normalized;
                Gizmos.color = Color.red;
                Gizmos.DrawLine(rayPos.position,
                               (rayPos.position) + directionToPlayer * rayDistance);
            }
        }
    }
}
