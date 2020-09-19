using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SaG.GuidReferences;
using SaG.SaveSystem.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SaG.SaveSystem.SaveableRuntimeInstances
{
    /// <summary>
    /// Each scene has a Save Instance Manager
    /// The responsibility for this manager is to keep track of all saved instances within that scene.
    /// Examples of saved instances are keys or items you have dropped out of your inventory.
    /// </summary>
    public class SceneRuntimeInstancesManager : ISceneRuntimeInstancesManager
    {
        private readonly Scene scene;
        private readonly IAssetResolver assetResolver;
        private Dictionary<SavedInstance, SpawnInfo> spawnInfo = new Dictionary<SavedInstance, SpawnInfo>();
        private HashSet<string> loadedIDs = new HashSet<string>();

        private int changesMade;

        public string Id { get; }
        public string Context { get; }

        public SceneRuntimeInstancesManager(Scene scene, IAssetResolver assetResolver)
        {
            this.scene = scene;
            this.assetResolver = assetResolver;
            // todo
            Id = scene.path;
            Context = scene.path;
        }
        
        [Serializable]
        private class SaveData
        {
            public SpawnInfo[] spawns;
        }

        [Serializable]
        private struct SpawnInfo
        {
            public InstanceSource source;
            public string assetId;
            public string guid;
        }

        #region ISceneRuntimeInstancesManager implementation

        public GameObject Instantiate(string assetId, InstanceSource source = InstanceSource.Resources)
        {
            changesMade++;
            var savedInstance = CreateSavedInstance(assetId, source);
            // we need to enable instance first so that GuidComponent will generate new Guid
            savedInstance.gameObject.SetActive(true);
            
            var saveable = savedInstance.GetComponent<Saveable>();

            spawnInfo.Add(savedInstance, new SpawnInfo()
            {
                assetId = assetId,
                guid = saveable.Id,
                source = source
            });

            return savedInstance.gameObject;
        }

        public void Destroy(SavedInstance savedInstance)
        {
            if (!savedInstance.IsDestroying)
            {
                savedInstance.Destroy(); // todo decide to remove or not this way of destroying saved instance... 
                return;
            }

            if (spawnInfo.ContainsKey(savedInstance))
            {
                spawnInfo.Remove(savedInstance);
                loadedIDs.Remove(savedInstance.Saveable.Id);

                changesMade++;
            }
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
        }

        #endregion ISceneRuntimeInstancesManager implementation

        
        #region ISaveable implementation

        private JObject lastSaveData;
        
        public JObject Save()
        {
            if (changesMade == 0 && lastSaveData != null)
                return lastSaveData;
            changesMade = 0;

            int c = spawnInfo.Count;

            SaveData data = new SaveData()
            {
                spawns = new SpawnInfo[c]
            };

            int i = 0;
            foreach (SpawnInfo item in spawnInfo.Values)
            {
                data.spawns[i] = item;
                i++;
            }

            lastSaveData = JObject.FromObject(data);
            return lastSaveData;
        }

        public void Load(JObject data)
        {
            SaveData saveData = data.ToObject<SaveData>();
            lastSaveData = data;

            if (saveData != null && saveData.spawns != null)
            {
                int itemCount = saveData.spawns.Length;

                for (int i = 0; i < itemCount; i++)
                {
                    if (loadedIDs.Contains(saveData.spawns[i].guid))
                    {
                        // this means we already spawned this game object
                        continue;
                    }

                    var source = saveData.spawns[i].source;
                    var path = saveData.spawns[i].assetId;
                    var id = saveData.spawns[i].guid;
                    if (Guid.TryParse(id, out var guid))
                    {
                        try
                        {
                            var obj = InstantiateInternal(path, guid, source);
                            spawnInfo.Add(obj, saveData.spawns[i]);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Were unable to spawn object from save data.\n" +
                                           $"guid: '{guid}'\nasset: '{path}'\nsource: '{source.ToString()}'");
                            Debug.LogException(e);
                        }
                    }
                    else
                    {
                        Debug.LogError($"Unknown guid '{id}' for asset '{path}'."); // todo scene info?
                    }
                }
            }
        }

        #endregion ISaveable implementation
        
        private SavedInstance InstantiateInternal(string assetId, Guid guid, InstanceSource source)
        {
            if (string.IsNullOrEmpty(assetId))
                throw new ArgumentNullException(nameof(assetId));
            if (guid == Guid.Empty)
                throw new ArgumentNullException(nameof(guid));

            var savedInstance = CreateSavedInstance(assetId, source);

            var guidProvider = savedInstance.GetComponent<GuidComponent>();
            guidProvider.SetGuid(guid);

            loadedIDs.Add(savedInstance.GetComponent<Saveable>().Id);

            savedInstance.gameObject.SetActive(true);

            return savedInstance;
        }

        private SavedInstance CreateSavedInstance(string assetId, InstanceSource source)
        {
            var prefab = assetResolver.Resolve(assetId, source);
            // We will temporarily set the resource to disabled. Because we don't want to enable any
            // of the components yet.
            bool prefabActiveState = prefab.gameObject.activeSelf;
            prefab.SetActive(false);

            GameObject instance = Object.Instantiate(prefab);
            SceneManager.MoveGameObjectToScene(instance, scene);

            // After instantiating we reset the resource back to it's original state.
            prefab.SetActive(prefabActiveState);

            Saveable saveable = instance.GetComponent<Saveable>();

            if (saveable == null)
            {
                Debug.LogWarning("Save Instance Manager: No saveable added to spawned object." +
                                 $" Scanning for ({nameof(ISaveableComponent)})s during runtime is more costly.");

                saveable = instance.AddComponent<Saveable>();
                saveable.ScanAddSaveableComponents();
            }

            var guidProvider = instance.GetComponent<GuidComponent>() ?? instance.AddComponent<GuidComponent>();

            SavedInstance savedInstance = instance.AddComponent<SavedInstance>();
            savedInstance.Configure(saveable, this);

            loadedIDs.Add(saveable.Id);
            return savedInstance;
        }
        
        
    }
}