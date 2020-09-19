using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using SaG.GuidReferences;
using SaG.SaveSystem.Components;
using SaG.SaveSystem.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

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

        public GameObject Instantiate(string assetId, InstanceSource source = InstanceSource.Resources, Scene scene = default)
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

            var instancesManager = new SceneRuntimeInstancesManager(scene, assetResolver);
            gameStateManager.RegisterSaveable(instancesManager, true); // todo settings?
            sceneRuntimeInstancesManagers.Add(sceneHashCode, instancesManager);
        }

        private void SceneManagerOnSceneUnloaded(Scene scene)
        {
            int sceneHashCode = scene.GetHashCode();
            if (sceneRuntimeInstancesManagers.TryGetValue(sceneHashCode, out var instancesManager))
            {
                // todo dispose instances manager?
                gameStateManager.UnregisterSaveable(instancesManager, true); // todo settings?
                sceneRuntimeInstancesManagers.Remove(sceneHashCode);
            }
        }
    }
}