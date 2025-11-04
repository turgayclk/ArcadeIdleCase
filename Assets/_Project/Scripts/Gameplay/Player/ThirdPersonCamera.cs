using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 8f, -6f);
    public float followSpeed = 10f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

        transform.LookAt(target.position);
    }
}
