using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Spawner : MonoBehaviour
{
    [SerializeField] private ItemDefinition itemDefinition;
    [SerializeField] private StorageArea outputStorage;
    [SerializeField] private float interval = 0.5f;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float animDuration = 0.5f;

    private Coroutine spawnRoutine;

    private void OnEnable()
    {
        if (itemDefinition == null) { Debug.LogError("ItemDefinition not assigned!", this); return; }
        if (outputStorage == null) { Debug.LogError("OutputStorage not assigned!", this); return; }

        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (PoolManager.Instance == null)
            yield return null;

        if (itemDefinition.prefab == null)
        {
            Debug.LogError("Prefab is null!", this);
            yield break;
        }

        while (true)
        {
            // Depo doluysa bekle
            if (outputStorage.Count >= outputStorage.Capacity)
            {
                yield return null;
                continue;
            }

            // Havuzdan obje çek
            GameObject obj = PoolManager.Instance.Get(itemDefinition.prefab);

            // Havuzda yok ? bekle, tekrar dene
            if (obj == null)
            {
                // Debug.Log("No pooled item available, waiting...");
                yield return null;
                continue;
            }

            var pooledItem = obj.GetComponent<PooledItem>();

            if (pooledItem == null)
            {
                Debug.LogError("Missing PooledItem component!");
                Destroy(obj);
                yield return new WaitForSeconds(interval);
                continue;
            }

            pooledItem.SetPrefabReference(itemDefinition.prefab);

            // --- ANÝMASYON ---
            Transform t = obj.transform;

            Vector3 startPos = spawnPoint != null ? spawnPoint.position : transform.position;
            t.position = startPos;
            t.localScale = Vector3.zero;

            t.DOScale(1f, 0.25f).SetEase(Ease.OutBack);

            Vector3 targetPos = outputStorage.GetNextFreeLocalPositionWorld();
            t.DOMove(targetPos, animDuration).SetEase(Ease.OutQuad);

            yield return new WaitForSeconds(animDuration);

            outputStorage.TryStore(pooledItem);

            yield return new WaitForSeconds(interval);
        }
    }


}
