using UnityEngine;

public class PooledItem : MonoBehaviour, ICarryable
{
    [SerializeField] private ItemDefinition definition;
    public ItemDefinition Definition => definition;

    private Transform _tr;

    private void Awake()
    {
        _tr = transform;
    }

    public void SetCarrierTransform(Transform parent, Vector3 localPos)
    {
        _tr.SetParent(parent);
        _tr.localPosition = localPos;
        _tr.localRotation = Quaternion.identity;
        gameObject.SetActive(true);
    }

    public void ReturnToPool()
    {
        _tr.SetParent(null);
        gameObject.SetActive(false);
    }
}
