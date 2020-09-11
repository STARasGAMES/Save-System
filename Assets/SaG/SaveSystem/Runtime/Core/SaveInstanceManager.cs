using System;
using System.Collections.Generic;
using SaG.SaveSystem.Components;
using SaG.SaveSystem.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaG.SaveSystem.Core
{
    /// <summary>
    /// Each scene has a Save Instance Manager
    /// The responsibility for this manager is to keep track of all saved instances within that scene.
    /// Examples of saved instances are keys or items you have dropped out of your inventory.
    /// </summary>
    [DefaultExecutionOrder(-9100), AddComponentMenu("")]
    public class SaveInstanceManager : MonoBehaviour, ISaveableComponent
    {
        private Dictionary<SavedInstance, SpawnInfo> spawnInfo = new Dictionary<SavedInstance, SpawnInfo>();
        private HashSet<string> loadedIDs = new HashSet<string>();

        private int spawnCountHistory;
        private int changesMade;

        public string SceneID { set; get; }
        public Saveable Saveable { set; get; }
        public int LoadedIDCount { get { return loadedIDs.Count; } }

        public Type SaveDataType => typeof(SaveData);

        [Serializable]
        private class SaveData
        {
            public SpawnInfo[] infoCollection;
            public int spawnCountHistory;
        }

        [Serializable]
        private struct SpawnInfo
        {
            public InstanceSource source;
            public string filePath;
            public string saveIdentification;
        }

        public void DestroyAllObjects()
        {
            List<SavedInstance> instances = new List<SavedInstance>();

            foreach (var item in spawnInfo)
            {
                if (item.Key != null)
                {
                    instances.Add(item.Key);
                }
            }

            int totalInstanceCount = instances.Count;
            for (int i = 0; i < totalInstanceCount; i++)
            {
                instances[i].Destroy();
            }

            spawnInfo.Clear();
            loadedIDs.Clear();
            spawnCountHistory = 0;
        }

        public void DestroyObject(SavedInstance savedInstance, Saveable saveable)
        {
            if (spawnInfo.ContainsKey(savedInstance))
            {
                spawnInfo.Remove(savedInstance);
                loadedIDs.Remove(saveable.Id);

                changesMade++;
            }
        }

        public SavedInstance SpawnObject(InstanceSource source, string filePath, string saveIdentification = "")
        {
            GameObject getResource = null;

            // Implement more spawn methods here.
            // Such as usage for Asset Bundles & Adressables
            switch (source)
            {
                case InstanceSource.Resources:

                    getResource = Resources.Load(filePath) as GameObject;

                    break;
                default:
                    break;
            }

            if (getResource == null)
            {
                Debug.LogWarning(string.Format("Invalid resource path: {0}", filePath));
                return null;
            }

            changesMade++;

            // We will temporarily set the resource to disabled. Because we don't want to enable any
            // of the components yet.
            bool resourceState = getResource.gameObject.activeSelf;
            getResource.gameObject.SetActive(false);

            GameObject instance = Instantiate(getResource, getResource.transform.position, getResource.transform.rotation);
            SceneManager.MoveGameObjectToScene(instance.gameObject, gameObject.scene);

            // After instantiating we reset the resource back to it's original state.
            getResource.gameObject.SetActive(resourceState);

            Saveable saveable = instance.GetComponent<Saveable>();

            if (saveable == null)
            {
                Debug.LogWarning("Save Instance Manager: No saveable added to spawned object." +
                    $" Scanning for ({nameof(ISaveableComponent)})s during runtime is more costly.");
                saveable = instance.AddComponent<Saveable>();
                saveable.ScanAddSaveableComponents();
            }

            SavedInstance savedInstance = instance.AddComponent<SavedInstance>();
            savedInstance.Configure(saveable, this);

            // In case the object has no idenfication, which applies to all prefabs.
            // Then we give it a new identification, and we store it into our spawninfo array so we know to spawn it again.
            saveIdentification = saveable.Id;
            if (string.IsNullOrEmpty(saveIdentification))
            {
                //saveable.Id = string.Format("{0}-{1}-{2}", SceneID, saveable.name, spawnCountHistory); // todo
                Debug.LogError($"Saveable has no id.", saveable);
                spawnInfo.Add(savedInstance, new SpawnInfo()
                {
                    filePath = filePath,
                    saveIdentification = saveable.Id,
                    source = source
                });

                spawnCountHistory++;

                loadedIDs.Add(saveable.Id);
            }
            else
            {
                //saveable.Id = saveIdentification; // todo
                loadedIDs.Add(saveable.Id);
            }

            instance.gameObject.SetActive(true);

            return savedInstance;
        }

        public object Save()
        {
            if (changesMade > 0)
            {
                changesMade = 0;

                int c = spawnInfo.Count;

                SaveData data = new SaveData()
                {
                    infoCollection = new SpawnInfo[c],
                    spawnCountHistory = spawnCountHistory
                };

                int i = 0;
                foreach (SpawnInfo item in spawnInfo.Values)
                {
                    data.infoCollection[i] = item;
                    i++;
                }
                return JsonUtility.ToJson(data, SaveSettings.Get().useJsonPrettyPrint);
            }
            else
            {
                return "";
            }
        }

        public void Load(object data)
        {
            SaveData saveData = (SaveData)data;

            if (saveData != null && saveData.infoCollection != null)
            {
                spawnCountHistory = saveData.spawnCountHistory;

                int itemCount = saveData.infoCollection.Length;

                for (int i = 0; i < itemCount; i++)
                {
                    if (loadedIDs.Contains(saveData.infoCollection[i].saveIdentification))
                    {
                        return;
                    }

                    var source = saveData.infoCollection[i].source;
                    var path = saveData.infoCollection[i].filePath;
                    var id = saveData.infoCollection[i].saveIdentification;

                    var obj = SpawnObject(source, path, id);

                    spawnInfo.Add(obj, saveData.infoCollection[i]);
                }

                // Compatibility for projects that did not save spawnCountHistory
                // Does not get executed for newer projects
                if (spawnCountHistory == 0 && itemCount != 0)
                {
                    foreach (var item in spawnInfo.Values)
                    {
                        string id = item.saveIdentification;
                        int getSpawnID = int.Parse(id.Substring(id.LastIndexOf('-') + 1));

                        if (getSpawnID > spawnCountHistory)
                        {
                            spawnCountHistory = getSpawnID + 1;
                        }
                    }
                }
            }
        }

        public bool OnSaveCondition()
        {
            return true;
        }
    }
}