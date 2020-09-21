using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace SaG.SaveSystem.Samples.RuntimeSpawn
{
    public class Sample : MonoBehaviour
    {
        [SerializeField] private string _prefabPath = default;
        
        public void Save()
        {
            var saveSystem = SaveSystemSingleton.Instance;
            var stopwatch = Stopwatch.StartNew();
            saveSystem.GameStateManager.SynchronizeState();
            Debug.Log($"Synchronized state in {stopwatch.ElapsedMilliseconds} ms.");
            saveSystem.WriteStateToStorage("runtime_spawn_save_sample");
            stopwatch.Restart();
            Debug.Log($"Written state to disk in {stopwatch.ElapsedMilliseconds} ms.");
        }

        public void Load()
        {
            var saveSystem = SaveSystemSingleton.Instance;
            var stopwatch = Stopwatch.StartNew();
            saveSystem.ReadStateFromStorage("runtime_spawn_save_sample");
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
            if (!Input.GetMouseButtonDown(0))
                return;
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray,out var hit))
            {
                var go = SaveSystemSingleton.Instance.RuntimeInstancesManager.Instantiate(_prefabPath);
                go.transform.position = hit.point;
                go.transform.localScale = Vector3.one * Random.Range(0.5f, 2f);
            }
        }
    }
}