using System.Collections.Generic;
using UnityEngine;

public class StorageArea : MonoBehaviour, IStorage
{
    [SerializeField] private int capacity = 10;
    [SerializeField] private Transform anchor;
    [SerializeField] private bool fifo = true; // true: sýrayla al (Spawner için) - false: stack gibi al (Transformer için)
    [SerializeField] private bool isInputStorage = false;  // inputStorage özelliði ekleyelim

    [SerializeField] private ItemDefinition allowedItem; // sadece bu item kabul edilir

    private readonly List<ICarryable> items = new();

    public int Capacity => capacity;
    public int Count => items.Count;

    public bool CanAccept(ICarryable item)
    {
        // Kapasite kontrolü
        if (Count >= Capacity) return false;

        // Input Storage ise tip kontrolü
        if (isInputStorage && allowedItem != null)
        {
            return item.Definition == allowedItem;
        }

        return true;
    }

    // Bu fonksiyonu güncelledik. inputStorage'da item koyma iþlemi sadece burada yapýlacak.
    public bool TryStore(ICarryable item)
    {
        if (item == null) return false;

        // Input Storage filtresi
        if (isInputStorage)
        {
            if (allowedItem != null && item.Definition != allowedItem)
            {
                #if UNITY_EDITOR
                Debug.LogWarning($"InputStorage rejected: {item.Definition.name}. Allowed: {allowedItem.name}");
                #endif
                return false;
            }

            if (items.Count >= capacity) return false;

            items.Add(item);
            LayoutItems();
            return true;
        }

        // Normal storage
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
            Debug.LogWarning("No items to take from storage.");
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

    public Vector3 GetNextFreeLocalPositionWorld()
    {
        float h = items.Count > 0 ? items[0].Definition.stackHeight : 0f;
        Vector3 localPos = new Vector3(0, h * items.Count, 0);
        return anchor.TransformPoint(localPos);
    }

    // Getter method for isInputStorage
    public bool IsInputStorage() => isInputStorage;
}
