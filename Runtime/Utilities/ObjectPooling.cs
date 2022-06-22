using System.Collections.Generic;
using UnityEngine;

namespace Unidice.Simulator.Utilities
{
    public class ObjectPooling : MonoBehaviour
    {
        public static ObjectPooling Instance { get; private set; }

        [SerializeField] private int _initialPoolSize = 15;

        public class Pool
        {
            public GameObject prefab;
            public Transform container;
            public readonly List<GameObject> objectPool = new List<GameObject>();
        }

        private readonly Dictionary<GameObject, Pool> _pools = new Dictionary<GameObject, Pool>();
        private readonly Dictionary<GameObject, Pool> _leased = new Dictionary<GameObject, Pool>();

        public void Awake()
        {
            if (!Instance)
                Instance = this;
            else
                Destroy(gameObject);
        }

        /// <summary>
        /// Fill our new pool into a new empty object
        /// </summary>
        private Pool CreatePool(GameObject prefab)
        {
            var pool = new Pool
            {
                prefab = prefab,
                container = new GameObject(prefab.name + " Pool").transform
            };
            pool.container.transform.parent = transform;
            ExtendPool(pool, _initialPoolSize);
            return pool;
        }

        /// <summary>
        /// Request an object from the pool by type
        /// </summary>
        public GameObject GetPooledObject(GameObject prefab)
        {
            if (_pools.TryGetValue(prefab, out var pool))
            {
                var instance = GetObjectFromPool(pool);
                instance.SetActive(true);
                return instance;
            }
            Debug.LogError($"Prefab {prefab.name} is not registered for pooling.");
            return null;
        }

        /// <summary>
        /// Return an object to the pool.
        /// </summary>
        public void ReturnObject(GameObject instance)
        {
            instance.SetActive(false);
      
            if (_leased.TryGetValue(instance, out var pool))
            {
                pool.objectPool.Add(instance);
                _leased.Remove(instance);
            }
            else
            {
                Debug.LogError($"Object {instance.name} is not from any pool or has already been returned.", instance);
            }
        }

        /// <summary>
        /// If an object exists within a specified pool that isn't already active, return the object
        /// </summary>
        private GameObject GetObjectFromPool(Pool pool)
        {
            foreach (var instance in pool.objectPool)
            {
                if (!instance)
                {
                    Debug.LogError($"Object from pool {pool.prefab.name} has been destroyed.");
                    pool.objectPool.Remove(instance);
                    continue;
                }
                pool.objectPool.Remove(instance);
                _leased.Add(instance, pool);
                return instance;
            }

            ExtendPool(pool, 5);
            return GetObjectFromPool(pool); // Note: This may cause a stack overflow if the instantiated object in extend pool are not available within the same frame as requested
        }

        /// <summary>
        /// Adds more objects to pool
        /// </summary>
        private static void ExtendPool(Pool pool, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                var instance = Instantiate(pool.prefab, pool.container);
                instance.name = pool.prefab.name;
                instance.SetActive(false);
                pool.objectPool.Add(instance);
            }
        }

        public void RegisterPrefab(GameObject prefab)
        {
            if (_pools.ContainsKey(prefab)) return; // Already registered
            var pool = CreatePool(prefab);
            _pools.Add(prefab, pool);
        }
    }
}
