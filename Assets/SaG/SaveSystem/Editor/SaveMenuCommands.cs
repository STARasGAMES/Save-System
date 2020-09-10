using System.IO;
using SaG.GuidReferences;
using SaG.SaveSystem.Components;
using SaG.SaveSystem.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaG.SaveSystem.Editor
{
    public class SaveMenuCommands
    {
        [MenuItem(itemName: "Saving/Open Save Location")]
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

        [MenuItem("Saving/Open Save Settings")]
        public static void OpenSaveSystemSettings()
        {
            Selection.activeInstanceID = SaveSettings.Get().GetInstanceID();
        }

        [MenuItem("Saving/Utility/Wipe Save Identifications (Active Scene)")]
        public static void WipeSceneSaveIdentifications()
        {
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

        [MenuItem("Saving/Utility/Wipe Save Identifications (Active Selection(s))")]
        public static void WipeActiveSaveIdentifications()
        {
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