using UnityEngine;

public class PooledItem : MonoBehaviour, ICarryable
{
    [SerializeField] private ItemDefinition definition;
    public ItemDefinition Definition => definition;

    private Transform _tr;
    private GameObject prefabReference; // Bu obje hangi prefabýn havuzuna dönecek?

    private void Awake()
    {
        _tr = transform;
    }

    /// <summary>
    /// PoolManager tarafýndan spawn edilirken atanmalý
    /// </summary>
    public void SetPrefabReference(GameObject prefab)
    {
        prefabReference = prefab;
    }

    public void SetCarrierTransform(Transform parent, Vector3 localPos)
    {
        _tr.SetParent(parent);
        _tr.localPosition = localPos;
        _tr.localRotation = Quaternion.identity;
        gameObject.SetActive(true);
    }

    public void ReturnToPool()
    {
        _tr.SetParent(null);

        if (prefabReference != null)
        {
            // PoolManager’a geri gönder
            PoolManager.Instance.Release(prefabReference, gameObject);
        }
        else
        {
            // Güvenlik önlemi – referans yoksa fallback
            gameObject.SetActive(false);
            Debug.LogWarning($"{name} returned to pool WITHOUT prefab reference! Assign SetPrefabReference() when spawning.");
        }
    }
}
