using UnityEngine;
using System.Collections;
using DG.Tweening;

public class AssetTransformer : MonoBehaviour
{
    [Header("Storage")]
    [SerializeField] private StorageArea inputStorage;
    [SerializeField] private StorageArea outputStorage;

    [Header("Recipe")]
    [SerializeField] private TransformerRecipe recipe;
    [SerializeField] private int parallel = 1;  // Ayný anda kaç dönüþüm olabilir

    [Header("Animation Points")]
    [SerializeField] private Transform inputSpawnPoint;   // Ýþleme alýnmadan önce gelecek yer
    [SerializeField] private Transform outputSpawnPoint;  // Output item'ýn doðacaðý yer

    private int _processing;

    private void Update()
    {
        if (inputStorage.Count > 0 && outputStorage.Count < outputStorage.Capacity)
        {
            while (_processing < parallel && inputStorage.Count > 0 && outputStorage.Count < outputStorage.Capacity)
            {
                if (inputStorage.TryTake(out var item))
                {
                    _processing++;
                    StartCoroutine(ProcessItem(item));
                }
                else break;
            }
        }
    }

    private IEnumerator ProcessItem(ICarryable consumed)
    {
        if (consumed == null)
        {
            Debug.LogWarning("Consumed item is null. Skipping transformation.");
            yield break;
        }

        Transform itemTr = (consumed as Component).transform;

        // --- INPUT -> Ýþleme Noktasýna Animasyon ---
        itemTr.SetParent(null);
        Vector3 processPos = inputSpawnPoint != null ? inputSpawnPoint.position : transform.position;

        itemTr.DOMove(processPos, 0.35f).SetEase(Ease.OutQuad);
        itemTr.DOScale(1.1f, 0.25f).SetLoops(2, LoopType.Yoyo);

        yield return new WaitForSeconds(0.4f);

        // --- Kaybolma (ReturnToPool Öncesi) ---
        itemTr.DOScale(0f, 0.25f).SetEase(Ease.InBack);
        yield return new WaitForSeconds(0.25f);

        consumed.ReturnToPool(); // Artýk havuza döndü

        // --- Üretim Süresi ---
        yield return new WaitForSeconds(recipe.processTime);

        // --- OUTPUT ITEM Spawn ---
        GameObject go = PoolManager.Instance.Get(recipe.outputItem.prefab);
        if (go == null)
        {
            Debug.LogError("Output prefab is null!");
            yield break;
        }

        var newItem = go.GetComponent<PooledItem>();
        newItem.SetPrefabReference(recipe.outputItem.prefab);

        Transform newTr = go.transform;
        newTr.position = outputSpawnPoint != null ? outputSpawnPoint.position : transform.position;
        newTr.localScale = Vector3.zero;

        // pop anim
        newTr.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(0.25f);

        // Output Storage pozisyonuna uçuþ
        Vector3 targetPos = outputStorage.GetNextFreeLocalPositionWorld();
        newTr.DOMove(targetPos, 0.35f).SetEase(Ease.OutQuad);

        yield return new WaitForSeconds(0.35f);

        // Depoya koy
        outputStorage.TryStore(newItem);

        _processing--;
    }
}
