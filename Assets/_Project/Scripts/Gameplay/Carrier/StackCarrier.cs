using System.Collections.Generic;
using UnityEngine;

public class StackCarrier : MonoBehaviour, ICarrier
{
    [SerializeField] private int capacity = 5;
    [SerializeField] private Transform stackAnchor;

    private readonly List<ICarryable> stack = new();

    public int Capacity => capacity;
    public int Count => stack.Count;

    public bool TryPickupFrom(IStorage storage, int amount = 1)
    {
        int moved = 0;

        while (moved < amount && Count < Capacity && storage.TryTake(out var item))
        {
            stack.Add(item);

            var localPos = new Vector3(0, item.Definition.stackHeight * (stack.Count - 1), 0);
            item.SetCarrierTransform(stackAnchor, localPos);

            moved++;
        }

        return moved > 0;
    }

    public bool TryDropTo(IStorage storage, int amount = 1)
    {
        int moved = 0;

        while (moved < amount && stack.Count > 0)
        {
            var item = stack[^1]; // son eleman
            if (!storage.CanAccept(item)) break;

            stack.RemoveAt(stack.Count - 1);
            storage.TryStore(item);

            moved++;
        }

        return moved > 0;
    }
}
