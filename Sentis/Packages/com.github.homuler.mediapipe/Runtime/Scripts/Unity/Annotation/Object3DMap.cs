// using UnityEngine;
// using System.Collections.Generic;
// using UnityEngine.Serialization;

// namespace Mediapipe.Unity
// {
// public class Object3DMap : MonoBehaviour
// {
    
//     [SerializeField] private List<string> ObjectsLabels;
//     [SerializeField] private List<GameObject> ObjectsPrefs;
//     private Dictionary<string, GameObject> prefabMap = new Dictionary<string, GameObject>();
    
//     private bool isDone = false; // Maps category names to 3D prefabs


//     public GameObject GetPrefabByCategory(string category)
//     {
//          if(isDone == false)
//          {
//               for (int i = 0; i < ObjectsLabels.Count; i++)            
//                {
//                   prefabMap.Add(ObjectsLabels[i], ObjectsPrefs[i]);
//                }
//                isDone = true;
//          }

//          if(prefabMap.ContainsKey(category))
//          {
//             return prefabMap[category];
//          }
//          else{
             
//             Debug.Log(isDone);            
//             Debug.Log($"Prefab not found for category :{category}");
//             return null;

//          }
//     }
   
// }

// }

using UnityEngine;
using System.Collections.Generic;

namespace Mediapipe.Unity
{
    public class Object3DMap : MonoBehaviour
    {
        [SerializeField] private List<string> objectLabels;
        [SerializeField] private List<GameObject> objectPrefabs;
        
        private Dictionary<string, GameObject> _prefabMap;
        private bool _isInitialized = false;

        // Initialize immediately when script is enabled
        private void OnEnable()
        {
            InitializeDictionary();
        }

        // Also initialize if awakened
        private void Awake()
        {
            InitializeDictionary();
        }

        [ContextMenu("Force Reinitialize")]
        public void InitializeDictionary()
        {
            if (_isInitialized && _prefabMap != null) return;
            
            _prefabMap = new Dictionary<string, GameObject>();
            _isInitialized = false; // Reset in case we're reinitializing

            // Validation
            if (objectLabels == null || objectPrefabs == null)
            {
                Debug.LogError("Object labels or prefabs lists are not assigned!", this);
                return;
            }
            
            if (objectLabels.Count != objectPrefabs.Count)
            {
                Debug.LogError($"Mismatched counts! Labels: {objectLabels.Count}, Prefabs: {objectPrefabs.Count}", this);
                return;
            }

            // Population
            for (int i = 0; i < objectLabels.Count; i++)
            {
                if (string.IsNullOrEmpty(objectLabels[i]))
                {
                    Debug.LogWarning($"Empty label at index {i}", this);
                    continue;
                }

                if (objectPrefabs[i] == null)
                {
                    Debug.LogWarning($"Null prefab for label '{objectLabels[i]}'", this);
                    continue;
                }

                try
                {
                    _prefabMap[objectLabels[i]] = objectPrefabs[i];
                    Debug.Log($"Mapped '{objectLabels[i]}' to {objectPrefabs[i].name}", this);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to map '{objectLabels[i]}': {e.Message}", this);
                }
            }

            _isInitialized = true;
            Debug.Log($"Initialized with {_prefabMap.Count} valid mappings", this);
        }

        public GameObject GetPrefabByCategory(string category)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("Dictionary not initialized! Forcing initialization...", this);
                InitializeDictionary();
            }

            if (string.IsNullOrEmpty(category))
            {
                Debug.LogWarning("Received empty category name", this);
                return null;
            }

            if (_prefabMap.TryGetValue(category, out var prefab))
            {
                return prefab;
            }

            Debug.LogWarning($"Prefab not found for '{category}'. Available: {string.Join(", ", _prefabMap.Keys)}", this);
            return null;
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            // Reset initialization when modified in editor
            _isInitialized = false;
        }
        #endif
    }

}