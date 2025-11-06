using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private StackCarrier carrier;
    [SerializeField] private float pickupCooldown = 0.2f;

    private IStorage currentStorage;
    private bool isInStorageArea = false;
    private float lastPickupTime = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IStorage>(out var storage))
        {
            currentStorage = storage;
            isInStorageArea = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IStorage>(out var storage))
        {
            if (currentStorage == storage)
            {
                currentStorage = null;
                isInStorageArea = false;
            }
        }
    }

    private void Update()
    {
        if (currentStorage == null || !isInStorageArea)
            return;

        if (Time.time - lastPickupTime < pickupCooldown)
            return;

        bool isInputStorage = currentStorage is StorageArea sa && sa.IsInputStorage();

        // INPUT STORAGE: Item býrak
        if (isInputStorage)
        {
            if (carrier.Count > 0)
            {
                // TryDropTo kullan - daha güvenli
                if (carrier.TryDropTo(currentStorage, 1))
                {
                    lastPickupTime = Time.time;
                }
            }
            return;
        }

        // NORMAL/OUTPUT STORAGE: Item topla
        if (carrier.Count < carrier.Capacity)
        {
            if (carrier.TryPickupFrom(currentStorage, 1))
            {
                lastPickupTime = Time.time;
            }
        }
    }
}