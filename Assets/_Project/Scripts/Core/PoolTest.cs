using UnityEngine;

public class PoolTest : MonoBehaviour
{
    public GameObject prefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var obj = PoolManager.Instance.Get(prefab);
            obj.transform.position = Random.insideUnitSphere * 1f;
        }
    }
}
