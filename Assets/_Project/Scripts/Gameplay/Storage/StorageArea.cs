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

    public bool CanAccept(ICarryable item) => Count < Capacity;

    // Bu fonksiyonu güncelledik. inputStorage'da item koyma iþlemi sadece burada yapýlacak.
    public bool TryStore(ICarryable item)
    {
        Debug.Log($"TryStore called. IsInputStorage: {isInputStorage}, item: {item}");

        if (item == null)
        {
            Debug.LogWarning("Item is null, cannot store!");
            return false;
        }

        // Eðer bu Input Storage ise ve allowedItem tanýmlandýysa, filtre uygula
        if (isInputStorage)
        {
            if (allowedItem != null && item.Definition != allowedItem)
            {
                Debug.LogWarning($"InputStorage rejected item {item.Definition.name}. Allowed only: {allowedItem.name}");
                return false;
            }

            if (items.Count >= capacity)
            {
                Debug.LogWarning("InputStorage full!");
                return false;
            }

            items.Add(item);
            LayoutItems();
            Debug.Log("Item stored in INPUT storage.");
            return true;
        }

        // Normal depolar için standart mantýk
        if (CanAccept(item))
        {
            items.Add(item);
            LayoutItems();
            Debug.Log("Item stored in regular storage.");
            return true;
        }

        Debug.LogWarning("Storage FULL!");
        return false;
    }

    public bool TryTake(out ICarryable item)
    {
        Debug.Log("TryTake called.");

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
        Debug.Log($"Item taken: {item}. Remaining items: {items.Count}");
        return true;
    }

    private void LayoutItems()
    {
        Debug.Log($"LayoutItems called. Total items: {items.Count}");

        float h = items.Count > 0 ? items[0].Definition.stackHeight : 0f;

        for (int i = 0; i < items.Count; i++)
        {
            var tr = (items[i] as Component).transform;

            // Log the position of each item being laid out
            Debug.Log($"Item {i} positioned at {tr.position}");

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
