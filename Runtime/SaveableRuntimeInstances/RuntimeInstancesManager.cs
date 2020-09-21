using System;
using System.Collections.Generic;
using SaG.SaveSystem.GameStateManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaG.SaveSystem.SaveableRuntimeInstances
{
    public class RuntimeInstancesManager : IRuntimeInstancesManager
    {
        private readonly IGameStateManager _gameStateManager;
        private readonly IAssetResolver _assetResolver;
        private readonly IDictionary<int, ISceneRuntimeInstancesManager> _sceneRuntimeInstancesManagers;
        
        public RuntimeInstancesManager(IGameStateManager gameStateManager, IAssetResolver assetResolver)
        {
            _gameStateManager = gameStateManager;
            _assetResolver = assetResolver;
            _sceneRuntimeInstancesManagers = new Dictionary<int, ISceneRuntimeInstancesManager>();
            SceneManager.sceneLoaded += SceneManagerOnSceneLoaded;
            SceneManager.sceneUnloaded += SceneManagerOnSceneUnloaded;
        }
        
        #region IRuntimeInstancesManager implementation

        public IAssetResolver AssetResolver => _assetResolver;

        public GameObject Instantiate(string assetId, AssetSource source = AssetSource.Resources, Scene scene = default)
        {
            if (scene == default)
                scene = SceneManager.GetActiveScene();
            if (!_sceneRuntimeInstancesManagers.TryGetValue(scene.GetHashCode(), out var instancesManager))
            {
                throw new ArgumentException($"Trying to create object in scene that does not exist yet. Scene: '{scene.path}'", nameof(scene));
            }

            return instancesManager.Instantiate(assetId, source);
        }

        #endregion IRuntimeInstancesManager implementation

        private void SceneManagerOnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            int sceneHashCode = scene.GetHashCode();
            if (_sceneRuntimeInstancesManagers.ContainsKey(sceneHashCode))
            {
                Debug.LogError($"Duplicated scenes are not supported for now. Scene: '{scene.path}', hashCode: '{sceneHashCode}'");
                return;
            }

            var instancesManager = new SceneRuntimeInstancesManager(scene, _assetResolver);
            _sceneRuntimeInstancesManagers.Add(sceneHashCode, instancesManager);
            
            _gameStateManager.RegisterSaveable(instancesManager, true); // todo settings?
        }

        private void SceneManagerOnSceneUnloaded(Scene scene)
        {
            int sceneHashCode = scene.GetHashCode();
            if (_sceneRuntimeInstancesManagers.TryGetValue(sceneHashCode, out var instancesManager))
            {
                // important: we don't auto-save instanceManager because now scene is completely unloaded and gone!
                _gameStateManager.UnregisterSaveable(instancesManager, false); 
                _sceneRuntimeInstancesManagers.Remove(sceneHashCode);
            }
        }
    }
}