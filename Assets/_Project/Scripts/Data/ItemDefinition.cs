using UnityEngine;

[CreateAssetMenu(menuName = "Idle/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    public string id;
    public GameObject prefab;         // Bu ürünün sahnedeki prefabý
    public float stackHeight = 0.2f;  // Üst üste dizilirken yükseklik
    public Sprite icon;               // (Ýleride UI için kullanýrýz)
}
