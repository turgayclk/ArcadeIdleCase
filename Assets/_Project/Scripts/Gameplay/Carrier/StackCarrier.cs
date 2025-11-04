using System.Collections.Generic;
using UnityEngine;

public class StackCarrier : MonoBehaviour, ICarrier
{
    [SerializeField] private int capacity = 5;
    [SerializeField] private Transform stackAnchor;
    [SerializeField] private float stackHeight = 0.1f;

    [SerializeField] private List<ICarryable> stack = new();

    public int Capacity => capacity;
    public int Count => stack.Count;

    public bool TryPickupFrom(IStorage storage, int amount = 1)
    {
        int moved = 0;
        while (moved < amount && Count < Capacity && storage.TryTake(out var item))
        {
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

        item = stack[^1]; // son item
        return true;
    }

    public bool TryDropTo(IStorage storage, int amount = 1)
    {
        int moved = 0;
        while (moved < amount && stack.Count > 0)
        {
            var item = stack[^1];
            if (!storage.CanAccept(item)) break;

            stack.RemoveAt(stack.Count - 1);
            storage.TryStore(item);
            moved++;
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

        item = stack[^1]; // Son elemaný al
        stack.RemoveAt(stack.Count - 1);
        return true;
    }
}