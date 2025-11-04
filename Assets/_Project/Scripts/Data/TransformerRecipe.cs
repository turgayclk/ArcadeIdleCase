using UnityEngine;

[CreateAssetMenu(menuName = "Idle/Transformer Recipe")]
public class TransformerRecipe : ScriptableObject
{
    public ItemDefinition inputItem;   // Makineye giren ürün
    public ItemDefinition outputItem;  // Dönüþtükten sonra çýkan ürün
    public float processTime = 1.5f;   // Dönüþüm süresi (saniye)
}
