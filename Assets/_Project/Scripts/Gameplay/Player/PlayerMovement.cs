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

    [Header("VFX")]
    public ParticleSystem runParticle;

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
        float speed = inputDir.magnitude;

        // ?? ANIMATION
        if (animator != null)
            animator.SetFloat("Speed", speed);

        // ?? PARTICLE PLAY / STOP
        HandleRunParticle(speed);

        // ?? MOVEMENT + ROTATION
        if (speed > 0.1f)
        {
            controller.Move(inputDir * moveSpeed * Time.deltaTime);

            Quaternion targetRot = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleRunParticle(float speed)
    {
        if (runParticle == null) return;

        bool shouldPlay = speed > 0.1f;

        if (shouldPlay && !runParticle.isPlaying)
            runParticle.Play();

        if (!shouldPlay && runParticle.isPlaying)
            runParticle.Stop();
    }
}
