using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaG.SaveSystem.SaveableRuntimeInstances
{
    public class RuntimeInstancesManager : IRuntimeInstancesManager
    {
        private readonly IGameStateManager gameStateManager;
        private readonly IAssetResolver assetResolver;
        private readonly IDictionary<int, ISceneRuntimeInstancesManager> sceneRuntimeInstancesManagers;
        public RuntimeInstancesManager(IGameStateManager gameStateManager, IAssetResolver assetResolver)
        {
            this.gameStateManager = gameStateManager;
            this.assetResolver = assetResolver;
            sceneRuntimeInstancesManagers = new Dictionary<int, ISceneRuntimeInstancesManager>();
            SceneManager.sceneLoaded += SceneManagerOnSceneLoaded;
            SceneManager.sceneUnloaded += SceneManagerOnSceneUnloaded;
        }
        
        #region IRuntimeInstancesManager implementation

        public GameObject Instantiate(string assetId, AssetSource source = AssetSource.Resources, Scene scene = default)
        {
            if (scene == default)
                scene = SceneManager.GetActiveScene();
            if (!sceneRuntimeInstancesManagers.TryGetValue(scene.GetHashCode(), out var instancesManager))
            {
                throw new ArgumentException($"Trying to create object in scene that does not exist yet. Scene: '{scene.path}'", nameof(scene));
            }

            return instancesManager.Instantiate(assetId, source);
        }

        #endregion IRuntimeInstancesManager implementation

        private void SceneManagerOnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            int sceneHashCode = scene.GetHashCode();
            if (sceneRuntimeInstancesManagers.ContainsKey(sceneHashCode))
            {
                Debug.LogError($"Duplicated scenes are not supported for now. Scene: '{scene.path}', hashCode: '{sceneHashCode}'");
                return;
            }

            var instancesManager = new SceneRuntimeInstancesManager(scene, assetResolver, gameStateManager);
            sceneRuntimeInstancesManagers.Add(sceneHashCode, instancesManager);
            
            gameStateManager.RegisterSaveable(instancesManager, true); // todo settings?
        }

        private void SceneManagerOnSceneUnloaded(Scene scene)
        {
            int sceneHashCode = scene.GetHashCode();
            if (sceneRuntimeInstancesManagers.TryGetValue(sceneHashCode, out var instancesManager))
            {
                // todo dispose instances manager?
                // important: we don't auto-save instanceManager because scene is completely unloaded and gone!
                gameStateManager.UnregisterSaveable(instancesManager, false); 
                sceneRuntimeInstancesManagers.Remove(sceneHashCode);
            }
        }
    }
}