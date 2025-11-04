using System.Collections.Generic;
using UnityEngine;

public class StorageArea : MonoBehaviour, IStorage
{
    [SerializeField] private int capacity = 10;
    [SerializeField] private Transform anchor;
    [SerializeField] private bool fifo = true; // true: sýrayla al (Spawner için) - false: stack gibi al (Transformer için)

    private readonly List<ICarryable> items = new();

    public int Capacity => capacity;
    public int Count => items.Count;

    public bool CanAccept(ICarryable item) => Count < Capacity;

    public bool TryStore(ICarryable item)
    {
        if (!CanAccept(item)) return false;

        items.Add(item);
        LayoutItems();
        return true;
    }

    public bool TryTake(out ICarryable item)
    {
        if (items.Count == 0)
        {
            item = null;
            return false;
        }

        int index = fifo ? 0 : items.Count - 1;
        item = items[index];
        items.RemoveAt(index);

        LayoutItems();
        return true;
    }

    private void LayoutItems()
    {
        float h = items.Count > 0 ? items[0].Definition.stackHeight : 0f;

        for (int i = 0; i < items.Count; i++)
        {
            var tr = (items[i] as Component).transform;
            tr.SetParent(anchor);
            tr.localPosition = new Vector3(0, h * i, 0);
            tr.localRotation = Quaternion.identity;
            tr.gameObject.SetActive(true);
        }
    }
}
