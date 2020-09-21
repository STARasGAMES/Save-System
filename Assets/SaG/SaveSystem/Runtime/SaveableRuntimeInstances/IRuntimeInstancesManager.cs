using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaG.SaveSystem.SaveableRuntimeInstances
{
    public interface IRuntimeInstancesManager
    {
        /// <summary>
        /// Instantiates game object which instance will be saved and restored at scene loading.
        /// </summary>
        /// <param name="assetId">An asset ID or path to load from Resources.</param>
        /// <param name="source">Source from where asset will be resolved.</param>
        /// <param name="scene">Scene where game object will be instantiated. Active scene by default.</param>
        /// <returns>instance of an asset</returns>
        GameObject Instantiate(string assetId, AssetSource source = AssetSource.Resources, Scene scene = default);
    }
}