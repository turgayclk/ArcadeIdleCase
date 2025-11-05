using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AIAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string speedParam = "Speed"; // Animator parametre adý
    [SerializeField] private float animationLerpSpeed = 8f;

    private Vector3 _lastPosition;
    private float _currentSpeed;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        _lastPosition = transform.position;
    }

    private void Update()
    {
        // Hareket hýzýný hesapla
        float distance = Vector3.Distance(transform.position, _lastPosition);
        float targetSpeed = distance / Time.deltaTime;

        // Daha smooth bir geçiþ
        _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, Time.deltaTime * animationLerpSpeed);
        animator.SetFloat(speedParam, _currentSpeed);

        _lastPosition = transform.position;
    }
}
