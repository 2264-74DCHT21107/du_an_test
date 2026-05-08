using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;

    [System.Serializable]
    public class PoolItem
    {
        public string tag;
        public GameObject prefab;
        public int initialSize = 20;
    }

    [Header("Object Pool Settings")]
    public List<PoolItem> poolItems;

    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        InitializePools();
    }


    private void InitializePools()
    {
        foreach (var item in poolItems)
        {
            if (string.IsNullOrEmpty(item.tag) || item.prefab == null)
            {
                Debug.LogWarning($"PoolItem bị thiếu thông tin: {item.tag}");
                continue;
            }

            if (pools.ContainsKey(item.tag)) continue;

            Queue<GameObject> queue = new Queue<GameObject>();

            for (int i = 0; i < item.initialSize; i++)
            {
                GameObject obj = Instantiate(item.prefab, transform);
                obj.name = $"{item.tag}_{i}";
                obj.SetActive(false);
                queue.Enqueue(obj);
            }

            pools.Add(item.tag, queue);
        }
    }

    public GameObject Spawn(string tag, Vector3 position, Quaternion rotation)
    {
        if (!pools.ContainsKey(tag))
        {
            Debug.LogError($"Pool '{tag}' không tồn tại!");
            return null;
        }

        Queue<GameObject> poolQueue = pools[tag];


        if (poolQueue.Count == 0)
        {
            PoolItem item = poolItems.Find(p => p.tag == tag);
            if (item != null)
            {
                GameObject newObj = Instantiate(item.prefab, transform);
                newObj.name = $"{tag}_Dynamic";
                poolQueue.Enqueue(newObj);
                Debug.LogWarning($"Pool '{tag}' hết, đã tạo thêm object.");
            }
        }

        GameObject obj = poolQueue.Dequeue();


        obj.transform.localPosition = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);


        poolQueue.Enqueue(obj);

        return obj;
    }


    public void ReturnToPool(GameObject obj, string tag)
    {
        if (obj == null) return;

        obj.SetActive(false);
        obj.transform.SetParent(transform);
        obj.transform.localPosition = Vector3.zero;

        if (pools.ContainsKey(tag))
        {
            pools[tag].Enqueue(obj);
        }
        else
        {
            Debug.LogWarning($"Tag '{tag}' không tồn tại trong pool. Destroy object.");
            Destroy(obj);
        }
    }


    public void ClearAllPools()
    {
        foreach (var queue in pools.Values)
        {
            while (queue.Count > 0)
            {
                GameObject obj = queue.Dequeue();
                if (obj != null) Destroy(obj);
            }
        }
        pools.Clear();
    }
}