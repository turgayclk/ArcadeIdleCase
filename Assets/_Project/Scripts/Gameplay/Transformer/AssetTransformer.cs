using UnityEngine;
using System.Collections;

public class AssetTransformer : MonoBehaviour
{
    [Header("Storage")]
    [SerializeField] private StorageArea inputStorage;
    [SerializeField] private StorageArea outputStorage;

    [Header("Recipe")]
    [SerializeField] private TransformerRecipe recipe;
    [SerializeField] private int parallel = 1;  // Ayný anda kaç dönüþüm yapýlacaðý

    private int _processing;  // Ayný anda iþleme alýnan item sayýsý

    void Update()
    {
        // InputStorage’a item eklendiðinde dönüþüm baþlayacak
        if (inputStorage.Count > 0 && outputStorage.Count < outputStorage.Capacity)
        {
            Debug.Log($"Processing items. Input storage count: {inputStorage.Count}, Output storage count: {outputStorage.Count}");

            // Ayný anda yapýlacak dönüþüm sayýsý kadar iþlem yapýlacak
            while (_processing < parallel && inputStorage.Count > 0 && outputStorage.Count < outputStorage.Capacity)
            {
                if (inputStorage.TryTake(out var item))
                {
                    _processing++;
                    StartCoroutine(ProcessItem(item));
                }
                else
                    break;
            }
        }
    }

    // Bir item'ý dönüþtürme iþlemi
    IEnumerator ProcessItem(ICarryable consumed)
    {
        if (consumed == null)
        {
            Debug.LogWarning("Consumed item is null. Skipping transformation.");
            yield break;
        }

        Debug.Log($"Starting transformation for item: {consumed}");

        // Girdi item'ýný havuza geri gönder
        consumed.ReturnToPool();

        // Dönüþüm süresi kadar bekle
        yield return new WaitForSeconds(recipe.processTime);

        // Çýktý item'ý oluþtur
        var go = PoolManager.Instance.Get(recipe.outputItem.prefab);
        if (go == null)
        {
            Debug.LogWarning("Output item prefab is null. Cannot create output item.");
            yield break;
        }

        var pooledItem = go.GetComponent<PooledItem>();
        if (pooledItem == null)
        {
            Debug.LogWarning("Pooled item is null. Cannot proceed with transformation.");
            yield break;
        }

        pooledItem.SetCarrierTransform(outputStorage.transform, Vector3.zero);
        if (outputStorage.TryStore(pooledItem))
        {
            Debug.Log("Item successfully transformed and stored in output storage.");
        }

        _processing--;
    }
}
