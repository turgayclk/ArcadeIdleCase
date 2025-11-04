using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public float rotationSpeed = 10f;

    [Header("Joystick Reference")]
    public Joystick joystick;

    [Header("Animation")]
    public Animator animator;

    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        float h = joystick.Horizontal;
        float v = joystick.Vertical;

        Vector3 inputDir = new Vector3(h, 0, v).normalized;

        // **Speed deðerini animatöre gönder**
        float speed = inputDir.magnitude;
        if (animator != null)
            animator.SetFloat("Speed", speed);

        // Hareket
        if (speed > 0.1f)
        {
            controller.Move(inputDir * moveSpeed * Time.deltaTime);

            // Karakter yönünü çevir
            Quaternion targetRot = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }
}
