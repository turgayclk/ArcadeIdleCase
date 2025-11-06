using UnityEngine;

public class BasicAIWorker : MonoBehaviour
{
    private enum State
    {
        GoToSource,     // Spawner_Storage noktasýna yürü (Wood toplama)
        PickFromSource, // Wood al
        GoToInput,      // Transformer_Input_Storage noktasýna yürü
        DropToInput,    // Wood býrak
        GoToOutput,     // NEW: Output storage'a yürü (Tile toplamak için)
        PickFromOutput, // NEW: Tile al
        GoToTrash,      // NEW: Çöp kutusuna yürü
        DropToTrash,    // NEW: Tile yok et
        IdleWait        // Bekle
    }

    [Header("References")]
    [SerializeField] private StackCarrier carrier;
    [SerializeField] private StorageArea sourceStorage;   // Wood Spawner Storage
    [SerializeField] private StorageArea inputStorage;    // Wood Input Storage
    [SerializeField] private StorageArea outputStorage;   // NEW: Tile Output Storage
    [SerializeField] private Transform trashPoint;        // NEW: Trash hedef noktasý (TrashCan önüne)

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float stopDistance = 0.75f;

    [Header("Behaviour")]
    [SerializeField] private float actionInterval = 0.25f;
    [SerializeField] private int batchAmount = 1;
    [SerializeField] private float idleRetryDelay = 0.75f;

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
        // ? Öncelik: Eðer Output Storage doluysa ? önce Tile temizle
        if (state == State.GoToSource || state == State.IdleWait)
        {
            if (outputStorage != null && outputStorage.Count > 0 && carrier.Count == 0)
            {
                state = State.GoToOutput;
            }
        }

        switch (state)
        {
            case State.GoToSource:
                MoveTowards(sourceStorage.transform.position);
                if (Reached(sourceStorage.transform.position))
                    state = State.PickFromSource;
                break;

            case State.PickFromSource:
                if (sourceStorage.Count == 0 && carrier.Count == 0)
                {
                    IdleNear(sourceStorage.transform.position);
                    break;
                }

                if (carrier.Count >= carrier.Capacity)
                {
                    state = State.GoToInput;
                    break;
                }

                if (Time.time - _lastActionTime >= actionInterval)
                {
                    bool picked = carrier.TryPickupFrom(sourceStorage, batchAmount);
                    _lastActionTime = Time.time;

                    if (!picked) IdleNear(sourceStorage.transform.position);
                }
                break;

            case State.GoToInput:
                MoveTowards(inputStorage.transform.position);
                if (Reached(inputStorage.transform.position))
                    state = State.DropToInput;
                break;

            case State.DropToInput:
                if (inputStorage.Count >= inputStorage.Capacity && carrier.Count > 0)
                {
                    IdleNear(inputStorage.transform.position);
                    break;
                }

                if (carrier.Count == 0)
                {
                    state = State.GoToSource;
                    break;
                }

                if (Time.time - _lastActionTime >= actionInterval)
                {
                    bool dropped = carrier.TryDropTo(inputStorage, batchAmount);
                    _lastActionTime = Time.time;

                    if (!dropped) IdleNear(inputStorage.transform.position);
                }
                break;

            /* ---------------- NEW TILE CLEAN-UP LOGIC ---------------- */

            case State.GoToOutput:
                MoveTowards(outputStorage.transform.position);
                if (Reached(outputStorage.transform.position))
                    state = State.PickFromOutput;
                break;

            case State.PickFromOutput:
                if (carrier.Count >= carrier.Capacity || outputStorage.Count == 0)
                {
                    state = State.GoToTrash;
                    break;
                }

                if (Time.time - _lastActionTime >= actionInterval)
                {
                    bool picked = carrier.TryPickupFrom(outputStorage, batchAmount);
                    _lastActionTime = Time.time;

                    if (!picked) state = State.GoToTrash;
                }
                break;

            case State.GoToTrash:
                MoveTowards(trashPoint.position);
                if (Reached(trashPoint.position))
                    state = State.DropToTrash;
                break;

            case State.DropToTrash:
                if (carrier.Count == 0)
                {
                    state = State.GoToSource;
                    break;
                }

                if (Time.time - _lastActionTime >= actionInterval)
                {
                    if (carrier.TryTake(out var item))
                        item.ReturnToPool(); // ?? yok et

                    _lastActionTime = Time.time;
                }
                break;

            /* ----------------------------------------------------------- */

            case State.IdleWait:
                if (Time.time - _lastIdlePing >= idleRetryDelay)
                {
                    _lastIdlePing = Time.time;
                    if (carrier.Count == 0) state = State.GoToSource;
                    else state = State.GoToInput;
                }
                break;
        }
    }

    private void IdleNear(Vector3 pos)
    {
        if (!Reached(pos))
            MoveTowards(pos);
        state = State.IdleWait;
    }

    private void MoveTowards(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position);
        dir.y = 0f;

        float dist = dir.magnitude;
        if (dist <= stopDistance)
        {
            RotateTowards(dir);
            return;
        }

        Vector3 moveDir = dir.normalized;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
        RotateTowards(moveDir);
    }

    private void RotateTowards(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            rotationSpeed * Time.deltaTime
        );
    }

    private bool Reached(Vector3 targetPos)
    {
        Vector3 flat = transform.position; flat.y = 0f;
        targetPos.y = 0f;
        return Vector3.Distance(flat, targetPos) <= stopDistance;
    }
}
