﻿using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SaG.SaveSystem.Samples.Platformer2D
{
    public static class Game
    {
        public static void Checkpoint()
        {
            Debug.Log("Checkpoint");
            var saveSystem = SaveSystemSingleton.Instance;
            saveSystem.GameStateManager.DefaultContainer.Set("scene", SceneManager.GetActiveScene().name);
            saveSystem.GameStateManager.SynchronizeState();
            saveSystem.WriteStateToDisk("checkpoint");
        }

        public static void ManualSave()
        {
            var saveSystem = SaveSystemSingleton.Instance;
            saveSystem.GameStateManager.DefaultContainer.Set("scene", SceneManager.GetActiveScene().name);
            saveSystem.GameStateManager.SynchronizeState();
            saveSystem.WriteStateToDisk("ManualSave");
            if (saveSystem.FileUtility.IsFileExist("checkpoint"))
                saveSystem.FileUtility.DeleteFile("checkpoint");
        }

        public static void KillPlayer()
        {
            Debug.Log("Kill Player");
            var saveSystem = SaveSystemSingleton.Instance;
            var checkpoint = saveSystem.FileUtility.IsFileExist("checkpoint");
            var manualSave = saveSystem.FileUtility.IsFileExist("ManualSave");
            if (checkpoint || manualSave)
            {
                if (checkpoint)
                    LoadGameFromFile("checkpoint");
                else
                    LoadGameFromFile("ManualSave");
            }
            else
            {
                SceneManager.LoadScene("Level1");
            }
        }
        
        public static void LoadGameFromFile(string fileName)
        {
            Debug.Log($"Load from file: {fileName}");
            var saveSystem = SaveSystemSingleton.Instance;
            if (!saveSystem.FileUtility.IsFileExist(fileName))
                throw new ArgumentException($"Trying to load game from file that does not exist: '{fileName}'",nameof(fileName));
            
            saveSystem.ReadStateFromDisk(fileName);
            saveSystem.GameStateManager.LoadContainer(saveSystem.GameStateManager.DefaultContainer);
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