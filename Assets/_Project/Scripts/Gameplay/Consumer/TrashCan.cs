using UnityEngine;

public class TrashCan : MonoBehaviour
{
    [SerializeField] private float destroyInterval = 0.3f; // Item yok etme hýzý
    [SerializeField] private ItemDefinition allowedItem;   // Sadece bu item yok edilebilir

    private bool isPlayerInside = false;
    private StackCarrier carrier;
    private float lastDestroyTime = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<StackCarrier>(out var playerCarrier))
        {
            carrier = playerCarrier;
            isPlayerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<StackCarrier>(out var playerCarrier) && carrier == playerCarrier)
        {
            isPlayerInside = false;
            carrier = null;
        }
    }

    private void Update()
    {
        if (!isPlayerInside || carrier == null)
            return;

        if (carrier.Count > 0 && Time.time - lastDestroyTime >= destroyInterval)
        {
            // Carrier’dan bakmadan item’ý çekmek yerine önce peek yapalým:
            if (carrier.Peek(out var topItem))
            {
                // Eðer item doðru türde deðilse yok etme
                if (allowedItem != null && topItem.Definition != allowedItem)
                {
                    // Ýstersen burada “wrong item” feedback’i verebiliriz
                    return;
                }

                // Doðru item ? yok et
                if (carrier.TryTake(out var item))
                {
                    item.ReturnToPool();
                    lastDestroyTime = Time.time;
                }
            }
        }
    }
}
