using UnityEngine;

public class BasicAIWorker : MonoBehaviour
{
    private enum State
    {
        GoToSource,     // Spawner_Storage noktasýna yürü
        PickFromSource, // Depodan yavaþ yavaþ al
        GoToInput,      // Transformer_Input_Storage noktasýna yürü
        DropToInput,    // Depoya yavaþ yavaþ býrak
        IdleWait        // Bekle (boþsa veya doluysa)
    }

    [Header("References")]
    [SerializeField] private StackCarrier carrier;        // AI’nýn çantasý
    [SerializeField] private StorageArea sourceStorage;   // Spawner_Storage (çýkan ham ürünler)
    [SerializeField] private StorageArea inputStorage;    // Transformer_Input_Storage (ham ürün giriþi)

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float stopDistance = 0.75f;

    [Header("Behaviour")]
    [SerializeField] private float actionInterval = 0.25f; // Al / býrak tick süresi
    [SerializeField] private int batchAmount = 1;          // Her tikte kaç item
    [SerializeField] private float idleRetryDelay = 0.75f; // Beklerken ne sýklýkla denesin

    private State state;
    private float _lastActionTime;
    private float _lastIdlePing;

    private void Reset()
    {
        carrier = GetComponent<StackCarrier>();
    }

    private void Start()
    {
        if (carrier == null) carrier = GetComponent<StackCarrier>();
        state = State.GoToSource;
    }

    private void Update()
    {
        switch (state)
        {
            case State.GoToSource:
                MoveTowards(sourceStorage.transform.position);
                if (Reached(sourceStorage.transform.position))
                    state = State.PickFromSource;
                break;

            case State.PickFromSource:
                // Kaynak boþsa ve elimiz de boþsa: bekle
                if (sourceStorage.Count == 0 && carrier.Count == 0)
                {
                    IdleNear(sourceStorage.transform.position);
                    break;
                }

                // Çanta dolduysa hedefe
                if (carrier.Count >= carrier.Capacity)
                {
                    state = State.GoToInput;
                    break;
                }

                // Aksiyon aralýðý
                if (Time.time - _lastActionTime >= actionInterval)
                {
                    // Depoda varsa al, yoksa kýsa bekle
                    bool picked = carrier.TryPickupFrom(sourceStorage, batchAmount);
                    _lastActionTime = Time.time;

                    if (!picked)
                    {
                        // Kaynak tamamen boþsa biraz bekleyip yine deneriz
                        IdleNear(sourceStorage.transform.position);
                    }
                }
                break;

            case State.GoToInput:
                MoveTowards(inputStorage.transform.position);
                if (Reached(inputStorage.transform.position))
                    state = State.DropToInput;
                break;

            case State.DropToInput:
                // Input depo dolu ve elimizde item var -> bekle
                if (inputStorage.Count >= inputStorage.Capacity && carrier.Count > 0)
                {
                    IdleNear(inputStorage.transform.position);
                    break;
                }

                // Çantamýz boþaldýysa tekrar kaynaða
                if (carrier.Count == 0)
                {
                    state = State.GoToSource;
                    break;
                }

                if (Time.time - _lastActionTime >= actionInterval)
                {
                    bool dropped = carrier.TryDropTo(inputStorage, batchAmount);
                    _lastActionTime = Time.time;

                    if (!dropped)
                    {
                        // input doluysa / alamýyorsa burada kýsa bekle
                        IdleNear(inputStorage.transform.position);
                    }
                }
                break;

            case State.IdleWait:
                if (Time.time - _lastIdlePing >= idleRetryDelay)
                {
                    _lastIdlePing = Time.time;
                    // Mantýklý bir sonraki state’e dön
                    if (carrier.Count == 0)
                    {
                        // Eþya yoksa önce kaynaða
                        state = State.GoToSource;
                    }
                    else
                    {
                        // Üzerimiz doluysa input’a
                        state = State.GoToInput;
                    }
                }
                break;
        }
    }

    private void IdleNear(Vector3 pos)
    {
        // Noktaya çok uzak deðilsek orada oyalan; uzaksa yaklaþ
        if (!Reached(pos))
            MoveTowards(pos);
        state = State.IdleWait;
    }

    private void MoveTowards(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position);
        dir.y = 0f;
        float dist = dir.magnitude;
        if (dist <= stopDistance) return;

        Vector3 step = dir.normalized * moveSpeed * Time.deltaTime;
        transform.position += step;

        if (step.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(step);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    private bool Reached(Vector3 targetPos)
    {
        Vector3 flat = transform.position; flat.y = 0f;
        targetPos.y = 0f;
        return Vector3.Distance(flat, targetPos) <= stopDistance;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (sourceStorage != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(sourceStorage.transform.position, 0.3f);
        }
        if (inputStorage != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(inputStorage.transform.position, 0.3f);
        }
    }
#endif
}
