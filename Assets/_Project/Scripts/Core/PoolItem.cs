using System;
using UnityEngine;

[Serializable]
public class PoolItem
{
    public GameObject prefab;
    public int preloadCount = 10;
}
