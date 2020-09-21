using System;
using System.Collections;
using System.Linq;
using SaG.SaveSystem.Samples.Platformer2D.Triggers;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SaG.SaveSystem.Samples.Platformer2D
{
    public static class GAME
    {
        private const string ManualSaveFileName = "sample_platformer_Manual_save_file";
        private const string CheckpointSaveFileName = "sample_platformer_Checkpoint_save_file";
        
        /// <summary>
        /// Method that represents entry point of your game.
        /// It might be loader scene, main menu or DI installer - it doesn't matter.
        /// </summary>
        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] // todo this attribute breaks loading other scene
        private static void Initialize()
        {
            StartFromLastSave();
        }
        
        public static void CheckpointSave()
        {
            Debug.Log("Checkpoint Save");
            var saveSystem = SaveSystemSingleton.Instance;
            saveSystem.GameStateManager.DefaultContainer.Set("scene", SceneManager.GetActiveScene().name);
            saveSystem.GameStateManager.SynchronizeState();
            saveSystem.WriteStateToStorage(CheckpointSaveFileName);
        }

        public static void ManualSave()
        {
            Debug.Log("Manual Save");
            var saveSystem = SaveSystemSingleton.Instance;
            saveSystem.GameStateManager.DefaultContainer.Set("scene", SceneManager.GetActiveScene().name);
            saveSystem.GameStateManager.SynchronizeState();
            saveSystem.WriteStateToStorage(ManualSaveFileName);
            if (saveSystem.Storage.ContainsKey(CheckpointSaveFileName))
                saveSystem.Storage.Remove(CheckpointSaveFileName);
        }

        public static void StartFromLastSave()
        {
            Debug.Log("Restarting from last save...");
            var saveSystem = SaveSystemSingleton.Instance;
            var checkpoint = saveSystem.Storage.ContainsKey(CheckpointSaveFileName);
            var manualSave = saveSystem.Storage.ContainsKey(ManualSaveFileName);
            if (checkpoint || manualSave)
            {
                if (checkpoint)
                    LoadGameFromFile(CheckpointSaveFileName);
                else
                    LoadGameFromFile(ManualSaveFileName);
            }
            else
            {
                SceneManager.LoadScene("Level1");
            }
        }

        public static void LoadGameFromCheckpoint()
        {
            LoadGameFromFile(CheckpointSaveFileName);
        }
        
        public static void LoadGameFromManualSave()
        {
            LoadGameFromFile(ManualSaveFileName);
        }
        
        public static void LoadGameFromFile(string fileName)
        {
            Debug.Log($"Load from file: {fileName}");
            var saveSystem = SaveSystemSingleton.Instance;
            if (!saveSystem.Storage.ContainsKey(fileName))
                throw new ArgumentException($"Trying to load game from file that does not exist: '{fileName}'",nameof(fileName));
            
            saveSystem.ReadStateFromStorage(fileName);
            saveSystem.GameStateManager.LoadSaveable(saveSystem.GameStateManager.DefaultContainer);
            var sceneName = saveSystem.GameStateManager.DefaultContainer.Get<string>("scene");
            
            // We need to set this to true, so that Saveables from unloading scene
            // will not save their state into current GameState. We don't need state where player is dead:)
            saveSystem.GameStateManager.IsIgnoringStateSynchronization = true;
            
            SceneManager.LoadScene(sceneName);
            SceneManager.sceneLoaded += SceneManagerOnSceneLoaded;
        }

        private static void SceneManagerOnSceneLoaded(Scene scene, LoadSceneMode arg1)
        {
            Debug.Log($"'{scene.name}' loaded.");
            var saveSystem = SaveSystemSingleton.Instance;
            saveSystem.GameStateManager.IsIgnoringStateSynchronization = false;
            saveSystem.GameStateManager.LoadState();
            SceneManager.sceneLoaded -= SceneManagerOnSceneLoaded;
        }

        public static void LoadScene(string sceneName)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(sceneName);
            CoroutineRunner.RunCoroutine(CallNextFrame(() =>
            {
                var stt = Object.FindObjectsOfType<SceneTransitionTrigger>()
                    .FirstOrDefault(s => s.SceneName == currentSceneName);
                if (stt != null)
                {
                    Object.FindObjectOfType<Player>().transform.position = stt.SpawnPoint;
                }
            }));
            // we don't need to reload state because saveables will automatically grab state information at Awake.
        }

        private static IEnumerator CallNextFrame(Action action)
        {
            yield return null;
            action?.Invoke();
        }
    }
}