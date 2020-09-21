using System.Diagnostics;
using System.Linq;
using SaG.SaveSystem.SaveableRuntimeInstances;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace SaG.SaveSystem.Samples.RuntimeSpawn
{
    public class Sample : MonoBehaviour
    {
        private const string SaveFileName = "sample_runtime_spawn_save_file";
        
        [SerializeField] private string _prefabPath = default;

        [Header("Registered asset")] [SerializeField]
        private string _prefabId = default;

        [SerializeField] private GameObject _prefab = default;

        private void Awake()
        {
            SaveSystemSingleton.Instance.RuntimeInstancesManager.AssetResolver.RegisterAsset(_prefabId, _prefab);
        }

        public void Save()
        {
            var saveSystem = SaveSystemSingleton.Instance;
            var stopwatch = Stopwatch.StartNew();
            saveSystem.GameStateManager.SynchronizeState();
            Debug.Log($"Synchronized state in {stopwatch.ElapsedMilliseconds} ms.");
            saveSystem.WriteStateToStorage(SaveFileName);
            stopwatch.Restart();
            Debug.Log($"Written state to disk in {stopwatch.ElapsedMilliseconds} ms.");
        }

        public void Load()
        {
            var saveSystem = SaveSystemSingleton.Instance;
            var stopwatch = Stopwatch.StartNew();
            saveSystem.ReadStateFromStorage(SaveFileName);
            Debug.Log($"Read state from disk in {stopwatch.ElapsedMilliseconds} ms.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ClearScene()
        {
            string goName = _prefabPath.Split('/').Last();
            foreach (var runtimeInstance in FindObjectsOfType<GameObject>().Where(go => go.name.Contains(goName)))
            {
                Destroy(runtimeInstance);
            }
        }

        private void Update()
        {
            bool sphere = Input.GetMouseButtonDown(0);
            bool cube = Input.GetMouseButtonDown(1);
            if (!sphere && !cube)
                return;
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                GameObject go = null;
                if (sphere)
                    go = SaveSystemSingleton.Instance.RuntimeInstancesManager.Instantiate(_prefabPath);
                if (cube)
                    go = SaveSystemSingleton.Instance.RuntimeInstancesManager.Instantiate(_prefabId,
                        AssetSource.Registered);
                go.transform.position = hit.point;
                go.transform.localScale = Vector3.one * Random.Range(0.5f, 1.5f);
            }
        }
    }
}