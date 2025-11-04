using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private StackCarrier carrier;
    private IStorage currentStorage;
    private bool isInStorageArea = false;  // Depo alanýnda olup olmadýðýný kontrol etmek için

    [SerializeField] private float pickupCooldown = 0.5f;  // Item almak için bekleme süresi
    [SerializeField] private float lastPickupTime = 0f;    // Son alým zamaný

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IStorage>(out var storage))
        {
            currentStorage = storage;
            isInStorageArea = true;  // Depo alanýna girdi
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IStorage>(out var storage))
        {
            if (currentStorage == storage)
            {
                currentStorage = null;
                isInStorageArea = false;  // Depo alanýndan çýkýldý
            }
        }
    }

    private void Update()
    {
        if (currentStorage == null || !isInStorageArea)
            return;

        bool isInputStorage = currentStorage is StorageArea sa && sa.IsInputStorage();

        // INPUT STORAGE ? BIRAK
        if (isInputStorage)
        {
            if (carrier.Count > 0 && Time.time - lastPickupTime >= pickupCooldown)
            {
                if (carrier.TryTake(out var item))
                {
                    if (currentStorage.TryStore(item))
                    {
                        lastPickupTime = Time.time;
                    }
                }
            }
            return; // input ise aþaðýya hiç inmesin
        }

        // OUTPUT / NORMAL STORAGE ? AL
        if (carrier.Count < carrier.Capacity && Time.time - lastPickupTime >= pickupCooldown)
        {
            if (carrier.TryPickupFrom(currentStorage, 1))
            {
                lastPickupTime = Time.time;
            }
        }
    }

}
