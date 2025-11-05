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
    [SerializeField] private int parallel = 1;  // Ayný anda kaç dönüþüm yapýlacaðý

    [Header("Animation Points")]
    [SerializeField] private Transform inputSpawnPoint;   // Ýþleme alýnmadan item buraya gelecek
    [SerializeField] private Transform outputSpawnPoint;  // Ýþlemden çýkan item önce burada doðacak

    private int _processing;  // Þu an kaç item iþleniyor

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

        // Component'i al
        Transform itemTransform = (consumed as Component).transform;

        // 1?? INPUT ? Ýþleme Noktasýna Animasyon
        itemTransform.SetParent(null);
        Vector3 targetPos = inputSpawnPoint != null ? inputSpawnPoint.position : transform.position;

        itemTransform.DOMove(targetPos, 0.35f).SetEase(Ease.OutQuad);
        itemTransform.DOScale(1.1f, 0.25f).SetLoops(2, LoopType.Yoyo);

        yield return new WaitForSeconds(0.4f);

        // 2?? Ýþleme Efekti: Kaybolma
        itemTransform.DOScale(0f, 0.3f).SetEase(Ease.InBack);

        // Tam kaybolmasýný bekle
        yield return new WaitForSeconds(0.3f);

        // Input item'ý yok et (havuzuna yolla)
        consumed.ReturnToPool();

        // 3?? Üretim Süresi (piþiyor)
        yield return new WaitForSeconds(recipe.processTime);

        // 4?? ÇIKTI ITEM ? Doðsun
        var go = PoolManager.Instance.Get(recipe.outputItem.prefab);
        if (go == null)
        {
            Debug.LogError("Output prefab is null, cannot spawn output item.");
            yield break;
        }

        var newItem = go.GetComponent<PooledItem>();
        Transform t = go.transform;

        // Output spawn noktasýnda minicik belsin
        t.position = outputSpawnPoint != null ? outputSpawnPoint.position : transform.position;
        t.localScale = Vector3.zero;
        t.DOScale(1f, 0.25f).SetEase(Ease.OutBack);

        // 5?? OUTPUT ? Output Storage Stack pozisyonuna uçsun
        yield return new WaitForSeconds(0.25f);

        Vector3 stackPos = outputStorage.GetNextFreeLocalPositionWorld();
        t.DOMove(stackPos, 0.35f).SetEase(Ease.OutQuad);

        yield return new WaitForSeconds(0.35f);

        // Son olarak depoya ekle
        outputStorage.TryStore(newItem);

        _processing--;
    }
}
