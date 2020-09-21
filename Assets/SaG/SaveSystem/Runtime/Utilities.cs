using UnityEngine;

namespace SaG.SaveSystem
{
    public static class Utilities
    {
        private static bool IsQuittingGame;

        static Utilities()
        {
            IsQuittingGame = false;
            Application.quitting += () => IsQuittingGame = true;
        }
        
        /// <summary>
        /// Returns if the object has been destroyed using GameObject.Destroy(obj).
        /// Will return false if it has been destroyed due to the game exiting or scene unloading.
        /// </summary>
        /// <param name="gameObject">Game object instance.</param>
        /// <returns><c>true</c> if object has been destroyed using Object.Destroy(); otherwise, <c>false</c>.</returns>
        public static bool IsGameObjectDisabledExplicitly(GameObject gameObject)
        {
            return gameObject.scene.isLoaded && !IsQuittingGame;
        }
    }
}