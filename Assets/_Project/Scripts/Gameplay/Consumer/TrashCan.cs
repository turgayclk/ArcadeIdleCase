using UnityEngine;
using DG.Tweening;

public class TrashCan : MonoBehaviour
{
    [SerializeField] private float destroyInterval = 0.3f;  // Item yok etme hýzý
    [SerializeField] private ItemDefinition allowedItem;    // Sadece bu item yok edilebilir
    [SerializeField] private Transform trashPoint;          // Item’ýn çöpe uçacaðý nokta

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
            // Önce tepedeki item’a bakalým
            if (carrier.Peek(out var topItem))
            {
                // Yanlýþ item ise yok etme
                if (allowedItem != null && topItem.Definition != allowedItem)
                    return;

                // Doðru item -> animasyonlu yok et
                if (carrier.TryTake(out var item))
                {
                    AnimateTrash(item);
                    lastDestroyTime = Time.time;
                }
            }
        }
    }

    private void AnimateTrash(ICarryable item)
    {
        Transform t = (item as Component).transform;

        // Ebeveynlikten çýkar
        t.SetParent(null);

        Vector3 targetPos = trashPoint != null ? trashPoint.position : transform.position;

        // 1) Çöpe doðru uç
        t.DOMove(targetPos, 0.25f).SetEase(Ease.InQuad);

        // 2) Küçülerek kaybol
        t.DOScale(0f, 0.25f).SetEase(Ease.InBack).OnComplete(() =>
        {
            item.ReturnToPool();
        });
    }
}
