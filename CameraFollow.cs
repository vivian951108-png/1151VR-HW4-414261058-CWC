
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;

    private Vector3 offset;

    void Start()
    {
        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        // Keep camera height fixed when car jumps
        desiredPosition.y = offset.y;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            Mathf.Clamp01(smoothSpeed * Time.deltaTime)
        );
    }
}
