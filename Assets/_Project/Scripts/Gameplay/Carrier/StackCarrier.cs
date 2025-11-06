using System.Collections.Generic;
using UnityEngine;

public class StackCarrier : MonoBehaviour, ICarrier
{
    [SerializeField] private int capacity = 5;
    [SerializeField] private Transform stackAnchor;
    [SerializeField] private float stackHeight = 0.1f;
    [SerializeField] private List<ICarryable> stack = new();
    [SerializeField] private ItemDefinition lockedItemType = null;

    public int Capacity => capacity;
    public int Count => stack.Count;

    public bool TryPickupFrom(IStorage storage, int amount = 1)
    {
        int moved = 0;

        while (moved < amount && Count < Capacity && storage.TryTake(out var item))
        {
            // Ýlk item alýnýyorsa tip kilitle
            if (lockedItemType == null)
            {
                lockedItemType = item.Definition;
            }
            // Kilitli tip ile uyuþmuyorsa geri koy
            else if (item.Definition != lockedItemType)
            {
                storage.TryStore(item);
                break;
            }

            stack.Add(item);
            var localPos = new Vector3(0, stackHeight * (stack.Count - 1), 0);
            item.SetCarrierTransform(stackAnchor, localPos);
            moved++;
        }

        return moved > 0;
    }

    public bool Peek(out ICarryable item)
    {
        if (stack.Count == 0)
        {
            item = null;
            return false;
        }

        item = stack[^1];
        return true;
    }

    public bool TryDropTo(IStorage storage, int amount = 1)
    {
        Debug.Log($"[StackCarrier] TryDropTo started. Stack count: {stack.Count}, Locked type: {lockedItemType?.name}");

        int moved = 0;

        while (moved < amount && stack.Count > 0)
        {
            var item = stack[^1];
            Debug.Log($"[StackCarrier] Trying to drop: {item.Definition.name}");

            // ÖNCELÝKLE storage kabul edip etmeyeceðini kontrol et
            if (!storage.CanAccept(item))
            {
                Debug.Log("[StackCarrier] Storage cannot accept item (full)");
                break;
            }

            // Storage'a eklemeyi dene - BAÞARISIZ OLURSA STACK'TEN ÇIKARMA!
            if (!storage.TryStore(item))
            {
                Debug.LogWarning($"[StackCarrier] Storage rejected {item.Definition.name} (wrong type). Keeping in stack.");
                break; // Stack'ten çýkarmadýk, sadece döngüden çýk
            }

            // SADECE BAÞARILI OLURSA stack'ten çýkar
            stack.RemoveAt(stack.Count - 1);
            moved++;

            Debug.Log($"[StackCarrier] Item dropped successfully. Remaining in stack: {stack.Count}");
        }

        // Stack GERÇEKTEN boþaldýysa kilidi kaldýr
        if (stack.Count == 0)
        {
            Debug.Log($"[StackCarrier] Stack empty, unlocking type (was: {lockedItemType?.name})");
            lockedItemType = null;
        }
        else
        {
            Debug.Log($"[StackCarrier] Stack not empty ({stack.Count} items), keeping lock: {lockedItemType?.name}");
        }

        return moved > 0;
    }

    public bool TryTake(out ICarryable item)
    {
        if (stack.Count == 0)
        {
            item = null;
            return false;
        }

        item = stack[^1];
        stack.RemoveAt(stack.Count - 1);

        // Stack boþaldýysa kilidi kaldýr
        if (stack.Count == 0)
        {
            lockedItemType = null;
        }

        return true;
    }
}