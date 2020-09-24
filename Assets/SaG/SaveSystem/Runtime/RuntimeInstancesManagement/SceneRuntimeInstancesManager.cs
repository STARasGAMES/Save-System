using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SaG.GuidReferences;
using SaG.SaveSystem.GameStateManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SaG.SaveSystem.RuntimeInstancesManagement
{
    /// <summary>
    /// Each scene has a Scene Runtime Instances Manager
    /// The responsibility for this manager is to keep track of all saved instances within that scene.
    /// Examples of saved instances are keys or items you have dropped out of your inventory.
    /// </summary>
    public class SceneRuntimeInstancesManager : ISceneRuntimeInstancesManager
    {
        private readonly Scene _scene;
        private readonly IAssetResolver _assetResolver;
        private readonly IGameStateManager _gameStateManager;
        private readonly Dictionary<SavedInstance, SpawnInfo> _spawnInfo = new Dictionary<SavedInstance, SpawnInfo>();
        private readonly HashSet<string> _loadedIDs = new HashSet<string>();

        private int _changesMade;

        public string Id { get; }
        public string Context { get; }

        public SceneRuntimeInstancesManager(Scene scene, IAssetResolver assetResolver)
        {
            _scene = scene;
            _assetResolver = assetResolver;
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
            public AssetSource source;
            public string assetId;
            public string guid;
        }

        #region ISceneRuntimeInstancesManager implementation

        public GameObject Instantiate(string assetId, AssetSource source = AssetSource.Resources)
        {
            _changesMade++;
            var savedInstance = CreateSavedInstance(assetId, source);
            // we need to enable instance first so that GuidComponent will generate new Guid
            savedInstance.gameObject.SetActive(true);
            
            var guidProvider = savedInstance.GetComponent<GuidComponent>();
            guidProvider.RegenerateGuid();

            _spawnInfo.Add(savedInstance, new SpawnInfo()
            {
                assetId = assetId,
                guid = guidProvider.GetStringGuid(),
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

            if (_spawnInfo.ContainsKey(savedInstance))
            {
                _spawnInfo.Remove(savedInstance);
                _loadedIDs.Remove(savedInstance.Saveable.Id);

                _changesMade++;
            }
        }
        
        public void DestroyAllObjects()
        {
            List<SavedInstance> instances = new List<SavedInstance>();

            foreach (var item in _spawnInfo)
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

            _spawnInfo.Clear();
            _loadedIDs.Clear();
        }

        #endregion ISceneRuntimeInstancesManager implementation

        
        #region ISaveable implementation

        private JObject _lastSaveData;
        
        public JObject Save()
        {
            if (_changesMade == 0 && _lastSaveData != null)
                return _lastSaveData;
            _changesMade = 0;

            int c = _spawnInfo.Count;

            SaveData data = new SaveData()
            {
                spawns = new SpawnInfo[c]
            };

            int i = 0;
            foreach (SpawnInfo item in _spawnInfo.Values)
            {
                data.spawns[i] = item;
                i++;
            }

            _lastSaveData = JObject.FromObject(data);
            return _lastSaveData;
        }

        public void Load(JObject data)
        {
            _lastSaveData = data;
            var saveData = _lastSaveData.ToObject<SaveData>();
         
            if (saveData != null && saveData.spawns != null)
            {
                int itemCount = saveData.spawns.Length;

                for (int i = 0; i < itemCount; i++)
                {
                    if (_loadedIDs.Contains(saveData.spawns[i].guid))
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
                            _spawnInfo.Add(obj, saveData.spawns[i]);
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
        
        private SavedInstance InstantiateInternal(string assetId, Guid guid, AssetSource source)
        {
            if (string.IsNullOrEmpty(assetId))
                throw new ArgumentNullException(nameof(assetId));
            if (guid == Guid.Empty)
                throw new ArgumentNullException(nameof(guid));

            var savedInstance = CreateSavedInstance(assetId, source);

            var guidProvider = savedInstance.GetComponent<GuidComponent>();
            guidProvider.SetGuid(guid);

            _loadedIDs.Add(savedInstance.GetComponent<Saveable>().Id);

            savedInstance.gameObject.SetActive(true);

            return savedInstance;
        }

        private SavedInstance CreateSavedInstance(string assetId, AssetSource source)
        {
            var prefab = _assetResolver.Resolve(assetId, source);
            // We will temporarily set the resource to disabled. Because we don't want to enable any
            // of the components yet.
            bool prefabActiveState = prefab.gameObject.activeSelf;
            prefab.SetActive(false);

            GameObject instance = Object.Instantiate(prefab);
            SceneManager.MoveGameObjectToScene(instance, _scene);

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

            return savedInstance;
        }
        
        
    }
}