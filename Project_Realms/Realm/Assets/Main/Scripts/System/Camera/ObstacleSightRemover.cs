using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class ObstacleSightRemover : MonoBehaviour
{
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField, Range(0f, 1f)] private float targetAlpha = 0f;

    private Dictionary<Renderer, float> originalAlphas = new Dictionary<Renderer, float>();
    private List<Renderer> currentlyTransparentObjects = new List<Renderer>();
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Obstacle") && other.TryGetComponent<Renderer>(out Renderer obstacleRenderer))
        {
            if (!originalAlphas.ContainsKey(obstacleRenderer))
            {
                originalAlphas[obstacleRenderer] = obstacleRenderer.material.color.a;
            }

            Color color = obstacleRenderer.material.color;
            color.a = Mathf.Lerp(color.a, targetAlpha, Time.deltaTime * fadeSpeed);
            obstacleRenderer.material.color = color;

            if (!currentlyTransparentObjects.Contains(obstacleRenderer))
            {
                currentlyTransparentObjects.Add(obstacleRenderer);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Renderer>(out Renderer obstacleRenderer))
        {
            if (originalAlphas.ContainsKey(obstacleRenderer))
            {
                Color color = obstacleRenderer.material.color;
                color.a = originalAlphas[obstacleRenderer];
                obstacleRenderer.material.color = color;
                currentlyTransparentObjects.Remove(obstacleRenderer);
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
                kvp.Key.material.color = color;
            }
        }
    }
}
