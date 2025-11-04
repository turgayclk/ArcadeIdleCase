using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    [SerializeField] private ItemDefinition itemDefinition;
    [SerializeField] private StorageArea outputStorage;
    [SerializeField] private float interval = 0.5f;
    private Coroutine spawnRoutine;

    private void OnEnable()
    {
        // Null kontrolleri
        if (itemDefinition == null)
        {
            Debug.LogError("ItemDefinition is not assigned!", this);
            return;
        }

        if (outputStorage == null)
        {
            Debug.LogError("OutputStorage is not assigned!", this);
            return;
        }

        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        // PoolManager hazýr olana kadar bekle
        while (PoolManager.Instance == null)
        {
            yield return null;
        }

        // Prefab kontrolü
        if (itemDefinition.prefab == null)
        {
            Debug.LogError("itemDefinition.prefab is null!", this);
            yield break;
        }

        while (true)
        {
            if (outputStorage.Count >= outputStorage.Capacity)
            {
                yield return null;
                continue;
            }

            var obj = PoolManager.Instance.Get(itemDefinition.prefab);

            if (obj == null)
            {
                Debug.LogError("PoolManager returned null object!", this);
                yield return new WaitForSeconds(interval);
                continue;
            }

            var pooledItem = obj.GetComponent<PooledItem>();

            if (pooledItem == null)
            {
                Debug.LogError("Spawned object doesn't have PooledItem component!", this);
                Destroy(obj);
                yield return new WaitForSeconds(interval);
                continue;
            }

            if (!outputStorage.TryStore(pooledItem))
            {
                pooledItem.ReturnToPool();
            }

            yield return new WaitForSeconds(interval);
        }
    }
}