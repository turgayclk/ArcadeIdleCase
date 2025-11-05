using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Spawner : MonoBehaviour
{
    [SerializeField] private ItemDefinition itemDefinition;
    [SerializeField] private StorageArea outputStorage;
    [SerializeField] private float interval = 0.5f;
    [SerializeField] private Transform spawnPoint;

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
            if (outputStorage.Count >= outputStorage.Capacity)
            {
                yield return null;
                continue;
            }

            GameObject obj = PoolManager.Instance.Get(itemDefinition.prefab);
            var pooledItem = obj.GetComponent<PooledItem>();

            if (pooledItem == null)
            {
                Debug.LogError("Missing PooledItem component!");
                Destroy(obj);
                yield return new WaitForSeconds(interval);
                continue;
            }

            // --- ? Animation Phase START ? ---
            Transform t = obj.transform;

            // spawn point varsa oraya koy, yoksa spawner'ýn konumunu kullan
            Vector3 startPos = spawnPoint != null ? spawnPoint.position : transform.position;
            t.position = startPos;
            t.localScale = Vector3.zero; // küçük doðsun

            // Storage’de yerleþeceði konumu hesaplatmak için önce geçici store yapmayacaðýz.
            // Objeyi animasyonla anchor pozisyonuna götüreceðiz.

            //Hoþ bir "pop" effect
            t.DOScale(1f, 0.25f).SetEase(Ease.OutBack);

            // output storage anchor pozisyonuna atlama (zýplayarak gitsin daha tatlý görünür)
            Vector3 targetPos = outputStorage.GetNextFreeLocalPositionWorld();
            t.DOMove(targetPos, 0.35f).SetEase(Ease.OutQuad);

            yield return new WaitForSeconds(0.35f); // anim bitene kadar bekle

            // Son olarak depoya ekle
            outputStorage.TryStore(pooledItem);

            // --- ? Animation Phase END ? ---

            yield return new WaitForSeconds(interval);
        }
    }
}
