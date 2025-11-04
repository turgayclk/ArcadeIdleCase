using UnityEngine;

public interface ICarryable
{
    ItemDefinition Definition { get; }

    /// <summary>
    /// Bir taþýyýcýya (Player veya AI) baðlanýp,
    /// stack içinde belirlenen local pozisyona yerleþtirilir.
    /// </summary>
    void SetCarrierTransform(Transform parent, Vector3 localPos);

    /// <summary>
    /// Item artýk kullanýlmýyorsa havuza geri gönderilir.
    /// </summary>
    void ReturnToPool();
}
