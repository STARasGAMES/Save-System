using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json.Linq;
using SaG.GuidReferences;
using SaG.SaveSystem.Components;
using SaG.SaveSystem.Data;
using SaG.SaveSystem.SaveableRuntimeInstances;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaG.SaveSystem.Core
{
    /// <summary>
    /// Responsible for notifying all Saveable components
    /// Asking them to send data or retrieve data from/to the SaveGame
    /// </summary>
    [AddComponentMenu(""), DefaultExecutionOrder(-9015)]
    public class SaveMaster : MonoBehaviour
    {
        private static SaveMaster instance;

        private static GameObject saveMasterTemplate;

        // Used to track duplicate scenes.
        private static Dictionary<string, int> loadedSceneNames = new Dictionary<string, int>();
        private static HashSet<int> duplicatedSceneHandles = new HashSet<int>();

        private static bool isQuittingGame;

        // Active save game data
        private static GameState _activeGameState = null;
        private static int activeSlot = -1;

        // All listeners
        private static List<Saveable> saveables = new List<Saveable>();

        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateInstance()
        {
            GameObject saveMasterObject = new GameObject("Save Master");
            saveMasterObject.AddComponent<SaveMaster>();

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            DontDestroyOnLoad(saveMasterObject);
        }

        /*  
        *  Instance managers exist to keep track of spawned objects.
        *  These managers make it possible to drop a coin, and when you reload the game
        *  the coin will still be there.
        */

        private static void OnSceneUnloaded(Scene scene)
        {
            if (_activeGameState == null)
                return;

            // If it is a duplicate scene, we just remove this handle.
            if (duplicatedSceneHandles.Contains(scene.GetHashCode()))
            {
                duplicatedSceneHandles.Remove(scene.GetHashCode());
            }
            else
            {
                if (loadedSceneNames.ContainsKey(scene.name))
                {
                    loadedSceneNames.Remove(scene.name);
                }
            }
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
        {
            if (_activeGameState == null)
                return;

            // Store a reference to a non-duplicate scene
            if (!loadedSceneNames.ContainsKey(scene.name))
            {
                loadedSceneNames.Add(scene.name, scene.GetHashCode());
            }
            else
            {
                // These scenes are marked as duplicates. They need special treatment for saving.
                duplicatedSceneHandles.Add(scene.GetHashCode());
            }
        }


        /// <summary>
        /// Returns if the object has been destroyed using GameObject.Destroy(obj).
        /// Will return false if it has been destroyed due to the game exiting or scene unloading.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static bool IsGameObjectDisabledExplicitly(GameObject gameObject)
        {
            return gameObject.scene.isLoaded && !isQuittingGame;
        }

        /// <summary>
        /// Returns the active slot. -1 means no slot is loaded
        /// </summary>
        /// <returns> Active slot </returns>
        public static int GetActiveSlot()
        {
            return activeSlot;
        }

        /// <summary>
        /// Checks if there are any unused save slots.
        /// </summary>
        /// <returns></returns>
        public static bool HasUnusedSlots()
        {
            return SaveFileUtility.GetAvailableSaveSlot() != -1;
        }

        public static int[] GetUsedSlots()
        {
            return SaveFileUtility.GetUsedSlots();
        }

        public static bool IsSlotUsed(int slot)
        {
            return SaveFileUtility.IsSlotUsed(slot);
        }

        public static DateTime GetSaveCreationTime(int slot)
        {
            if (slot == activeSlot)
            {
                return _activeGameState.creationDate;
            }

            if (!IsSlotUsed(slot))
            {
                return new DateTime();
            }

            return GetSave(slot, true).creationDate;
        }

        public static DateTime GetSaveCreationTime()
        {
            return GetSaveCreationTime(activeSlot);
        }

        public static TimeSpan GetSaveTimePlayed(int slot)
        {
            if (slot == activeSlot)
            {
                return _activeGameState.timePlayed;
            }

            if (!IsSlotUsed(slot))
            {
                return new TimeSpan();
            }

            return GetSave(slot, true).timePlayed;
        }

        public static TimeSpan GetSaveTimePlayed()
        {
            return GetSaveTimePlayed(activeSlot);
        }

        public static int GetSaveVersion(int slot)
        {
            if (slot == activeSlot)
            {
                return _activeGameState.gameVersion;
            }

            if (!IsSlotUsed(slot))
            {
                return -1;
            }

            return GetSave(slot, true).gameVersion;
        }

        public static int GetSaveVersion()
        {
            return GetSaveVersion(activeSlot);
        }

        private static GameState GetSave(int slot, bool createIfEmpty = true)
        {
            if (slot == activeSlot)
            {
                return _activeGameState;
            }

            return SaveFileUtility.LoadSave(slot, createIfEmpty);
        }

        /// <summary>
        /// Automatically done on application quit or pause.
        /// Exposed in case you still want to manually write the active save.
        /// </summary>
        public static void WriteActiveSaveToDisk()
        {
            throw new NotImplementedException();
        }

        public static void LoadActiveSaveFromDisk(bool reloadSaveables = true)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete a save file based on a specific slot.
        /// </summary>
        /// <param name="slot"></param>
        public static void DeleteSave(int slot)
        {
            SaveFileUtility.DeleteSave(slot);

            if (slot == activeSlot)
            {
                activeSlot = -1;
                _activeGameState = null;
            }
        }

        /// <summary>
        /// Removes the active save file. Based on the save slot index.
        /// </summary>
        public static void DeleteSave()
        {
            DeleteSave(activeSlot);
        }

        /// <summary>
        /// Set a integer value in the current currently active save
        /// </summary>
        /// <param name="key"> Identifier to remember storage point </param>
        /// <param name="value"> Value to store </param>
        public static void SetInt(string key, int value)
        {
            if (HasActiveSaveLogAction("Set Int") == false) return;
            _activeGameState.Set(string.Format("IVar-{0}", key), value.ToString(), "Global");
        }

        /// <summary>
        /// Get an integer value in the currently active save
        /// </summary>
        /// <param name="key"> Identifier to remember storage point </param>
        /// <param name="defaultValue"> In case it fails to obtain the value, return this value </param>
        /// <returns> Stored value </returns>
        public static int GetInt(string key, int defaultValue = -1)
        {
            if (HasActiveSaveLogAction("Get Int") == false) return defaultValue;
            var getData = (string) _activeGameState.Get(string.Format("IVar-{0}", key));
            return string.IsNullOrEmpty((getData)) ? defaultValue : int.Parse(getData);
        }

        /// <summary>
        /// Set a floating point value in the currently active save
        /// </summary>
        /// <param name="key"> Identifier for value </param>
        /// <param name="value"> Value to store </param>
        public static void SetFloat(string key, float value)
        {
            if (HasActiveSaveLogAction("Set Float") == false) return;
            _activeGameState.Set(string.Format("FVar-{0}", key), value.ToString(), "Global");
        }

        /// <summary>
        /// Get a float value in the currently active save
        /// </summary>
        /// <param name="key"> Identifier to remember storage point </param>
        /// <param name="defaultValue"> In case it fails to obtain the value, return this value </param>
        /// <returns> Stored value </returns>
        public static float GetFloat(string key, float defaultValue = -1)
        {
            if (HasActiveSaveLogAction("Get Float") == false) return defaultValue;
            var getData = (string) _activeGameState.Get(string.Format("FVar-{0}", key));
            return string.IsNullOrEmpty((getData)) ? defaultValue : float.Parse(getData);
        }

        /// <summary>
        /// Sets a string value in the currently active save.
        /// </summary>
        /// <param name="key"> Identifier for value </param>
        /// <param name="value"> Value to store </param>
        public static void SetString(string key, string value)
        {
            if (HasActiveSaveLogAction("Set String") == false) return;
            _activeGameState.Set(string.Format("SVar-{0}", key), value, "Global");
        }

        /// <summary>
        /// Gets a string value in the currently active save.
        /// </summary>
        /// <param name="key"> Identifier to remember storage point </param>
        /// <param name="defaultValue"> In case it fails to obtain the value, return this value </param>
        /// <returns> Stored value </returns>
        public static string GetString(string key, string defaultValue = "")
        {
            if (HasActiveSaveLogAction("Get String") == false) return defaultValue;
            var getData = (string) _activeGameState.Get(string.Format("SVar-{0}", key));
            return string.IsNullOrEmpty((getData)) ? defaultValue : getData;
        }

        private static bool HasActiveSaveLogAction(string action)
        {
            if (GetActiveSlot() == -1)
            {
                Debug.LogWarning(string.Format("{0} Failed: no save slot set. Please call SetSaveSlot(int index)",
                    action));
                return false;
            }
            else return true;
        }

        // Events

        /// <summary>
        /// Gets called after current saveables gets saved and written to disk.
        /// You can start loading scenes based on this callback.
        /// </summary>
        public static Action<int> SlotChanging
        {
            get { return instance.onSlotChangeBegin; }
            set { instance.onSlotChangeBegin = value; }
        }

        public static Action<int> SlotChanged
        {
            get { return instance.onSlotChangeDone; }
            set { instance.onSlotChangeDone = value; }
        }

        private Action<int> onSlotChangeBegin = delegate { };
        private Action<int> onSlotChangeDone = delegate { };
        private Action<int> onWritingToDiskBegin = delegate { };
        private Action<int> onWritingToDiskDone = delegate { };

        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogWarning("Duplicate save master found. " +
                                 "Ensure that the save master has not been added anywhere in your scene.");
                Destroy(gameObject);
                return;
            }

            instance = this;

            var settings = SaveSettings.Get();

            if (settings.loadDefaultSlotOnStart)
            {
                // SetSlot(settings.defaultSlot, true);
            }

            if (settings.trackTimePlayed)
            {
                StartCoroutine(IncrementTimePlayed());
            }

            if (settings.useHotkeys)
            {
                StartCoroutine(TrackHotkeyUsage());
            }

            if (settings.saveOnInterval)
            {
                StartCoroutine(AutoSaveGame());
            }
        }

        private IEnumerator AutoSaveGame()
        {
            WaitForSeconds wait = new WaitForSeconds(SaveSettings.Get().saveIntervalTime);

            while (true)
            {
                yield return wait;
                WriteActiveSaveToDisk();
            }
        }

        private IEnumerator TrackHotkeyUsage()
        {
            var settings = SaveSettings.Get();

            while (true)
            {
                yield return null;

                if (!settings.useHotkeys)
                {
                    continue;
                }

                if (Input.GetKeyDown(settings.wipeActiveSceneData))
                {
                    // WipeSceneData(SceneManager.GetActiveScene().name);
                }

                if (Input.GetKeyDown(settings.saveAndWriteToDiskKey))
                {
                    var stopWatch = new System.Diagnostics.Stopwatch();
                    stopWatch.Start();

                    WriteActiveSaveToDisk();

                    stopWatch.Stop();
                    Debug.Log(string.Format("Synced objects & Witten game to disk. MS: {0}",
                        stopWatch.ElapsedMilliseconds.ToString()));
                }

                if (Input.GetKeyDown(settings.syncSaveGameKey))
                {
                    var stopWatch = new System.Diagnostics.Stopwatch();
                    stopWatch.Start();

                    // SyncSave();

                    stopWatch.Stop();
                    Debug.Log(string.Format("Synced (Save) objects. MS: {0}",
                        stopWatch.ElapsedMilliseconds.ToString()));
                }

                if (Input.GetKeyDown(settings.syncLoadGameKey))
                {
                    var stopWatch = new System.Diagnostics.Stopwatch();
                    stopWatch.Start();

                    // SyncLoad();

                    stopWatch.Stop();
                    Debug.Log(string.Format("Synced (Load) objects. MS: {0}",
                        stopWatch.ElapsedMilliseconds.ToString()));
                }
            }
        }

        private IEnumerator IncrementTimePlayed()
        {
            while (true)
            {
                yield return null;

                if (activeSlot >= 0)
                {
                    _activeGameState.timePlayed = _activeGameState.timePlayed.Add(TimeSpan.FromSeconds(Time.deltaTime));
                }
            }
        }

        // This will get called on android devices when they leave the game
        private void OnApplicationPause(bool pause)
        {
            if (!SaveSettings.Get().autoSaveOnExit)
                return;

            WriteActiveSaveToDisk();
        }

        private void OnApplicationQuit()
        {
            if (!SaveSettings.Get().autoSaveOnExit)
                return;

            isQuittingGame = true;
            WriteActiveSaveToDisk();
        }
    }
}