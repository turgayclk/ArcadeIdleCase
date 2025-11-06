using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    [Header("Pool Setup")]
    public List<PoolItem> poolItems;             // Inspector’dan preload listesi
    private Dictionary<GameObject, Queue<GameObject>> pools = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializePools();
    }

    private void InitializePools()
    {
        foreach (var item in poolItems)
        {
            if (item.prefab == null) continue;

            Queue<GameObject> queue = new Queue<GameObject>();
            pools[item.prefab] = queue;

            for (int i = 0; i < item.preloadCount; i++)
            {
                GameObject obj = Instantiate(item.prefab);
                obj.SetActive(false);
                obj.transform.SetParent(transform);
                queue.Enqueue(obj);
            }
        }
    }

    public GameObject Get(GameObject prefab)
    {
        if (!pools.TryGetValue(prefab, out var queue))
        {
            queue = new Queue<GameObject>();
            pools[prefab] = queue;
        }

        if (queue.Count > 0)
        {
            GameObject obj = queue.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        // Pool boþ ? yeni üretmeyeceðiz
        return null;
    }

    public void Release(GameObject prefab, GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform);

        if (!pools.ContainsKey(prefab))
            pools[prefab] = new Queue<GameObject>();

        pools[prefab].Enqueue(obj);
    }
}
