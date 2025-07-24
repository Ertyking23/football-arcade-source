using UnityEngine;

public class BounceTest : MonoBehaviour
{
    [Header("Bounce Settings")]
    [Tooltip("How high the capsule will bounce")]
    public float bounceHeight = 1.0f;

    [Range(0.1f, 5f)]
    public float bounceSpeed = 2.0f;

    [Space(10)]
    [SerializeField]
    private bool enableBouncing = true;

    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.position;
    }

    void Update()
    {
        if (enableBouncing)
        {
            float yOffset = Mathf.Abs(Mathf.Sin(Time.time * bounceSpeed)) * bounceHeight;
            transform.position = originalPosition + Vector3.up * yOffset;
        }
    }
}