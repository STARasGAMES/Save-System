using System.IO;
using SaG.GuidReferences;
using SaG.SaveSystem.GameStateManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaG.SaveSystem.Editor
{
    public class SaveMenuCommands
    {
        private const string MenuCommandsRoot = "Tools/Saving/";
        [MenuItem(MenuCommandsRoot + "Open Save Location")]
        public static void OpenSaveLocation()
        {
            string dataPath = string.Format("{0}/{1}/", Application.persistentDataPath, SaveSettings.Get().fileFolderName);

#if UNITY_EDITOR_WIN
            dataPath = dataPath.Replace(@"/", @"\"); // Windows uses backward slashes
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            dataPath = dataPath.Replace("\\", "/"); // Linux and MacOS use forward slashes
#endif

            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            EditorUtility.RevealInFinder(dataPath);
        }

        [MenuItem(MenuCommandsRoot + "Open Save Settings")]
        public static void OpenSaveSystemSettings()
        {
            Selection.activeInstanceID = SaveSettings.Get().GetInstanceID();
        }

        [MenuItem(MenuCommandsRoot + "Wipe Active Scene's GUIDs")]
        public static void WipeSceneGuids()
        {
            bool dialogResult = EditorUtility.DisplayDialog("Wipe Active Scene's GUIDs",
                "Are you sure you want to regenerate all GUIDs (Global Unique IDentifiers) from currently active scene? " +
                "This action cannot be undone!", "Yes, I'm sure", "No");
            if (!dialogResult)
                return;
            var activeScene = SceneManager.GetActiveScene();
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            int rootObjectCount = rootObjects.Length;

            // Get all Saveables, including children and inactive.
            for (int i = 0; i < rootObjectCount; i++)
            {
                foreach (Saveable item in rootObjects[i].GetComponentsInChildren<Saveable>(true))
                {
                    //item.Id = ""; todo
                    item.GetComponent<GuidComponent>().RegenerateGuid();
                    item.OnValidate();
                }
            }
        }

        [MenuItem(MenuCommandsRoot + "Wipe Selected Objects' GUIDs")]
        public static void WipeSelectedObjectsGuids()
        {
            bool dialogResult = EditorUtility.DisplayDialog("Wipe Selected Objects' GUIDs",
                "Are you sure you want to regenerate all GUIDs (Global Unique IDentifiers) for all currently selected objects? " +
                "This action cannot be undone!", "Yes, I'm sure", "No");
            if (!dialogResult)
                return;
            foreach (GameObject obj in Selection.gameObjects)
            {
                foreach (Saveable item in obj.GetComponentsInChildren<Saveable>(true))
                {
                    //item.Id = ""; todo
                    item.GetComponent<GuidComponent>().RegenerateGuid();
                    item.OnValidate();
                }
            }
        }
    }
}