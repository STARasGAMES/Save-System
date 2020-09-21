using System.Diagnostics;
using SaG.SaveSystem.GameStateManagement;
using UnityEngine;
using UnityEngine.UI;

namespace SaG.SaveSystem.Samples.SpeedTest
{
    public class SaveSpeedTest : MonoBehaviour
    {
        private const string SaveFileName = "sample_speed_test_save_file";
        
        [SerializeField] private Button buttonTestSynchronizeState = null;
        [SerializeField] private Button buttonTestLoadState = null;
        [SerializeField] private Button buttonTestWriteStateToDisk = null;
        [SerializeField] private Button buttonTestReadStateFromDisk = null;
        [SerializeField] private Button buttonMakeSaveablesDirty = null;
        [SerializeField] private Button buttonWipeAllSaveablesData = null;
        [SerializeField] private Button buttonDeleteSaveFile = null;
        [SerializeField] private Text displayText = null;

        [SerializeField] private GameObject saveablesContainer = null;
        private Saveable[] saveables;

        // For scene reload
        private static float lastestSpeed;

        private void Awake()
        {
            saveables = saveablesContainer.GetComponentsInChildren<Saveable>();

            buttonTestSynchronizeState.onClick.AddListener(SynchronizeState);
            buttonTestLoadState.onClick.AddListener(LoadState);
            buttonTestWriteStateToDisk.onClick.AddListener(WriteStateToStorage);
            buttonTestReadStateFromDisk.onClick.AddListener(ReadStateFromStorage);
            buttonMakeSaveablesDirty.onClick.AddListener(MakeSaveablesDirty);
            buttonDeleteSaveFile.onClick.AddListener(WipeSave);
            buttonWipeAllSaveablesData.onClick.AddListener(WipeSaveables);

            displayText.text = lastestSpeed.ToString();
        }

        private void SynchronizeState()
        {
            var stopWatch = Stopwatch.StartNew();

            SaveSystemSingleton.Instance.GameStateManager.SynchronizeState();
            
            stopWatch.Stop();
            displayText.text = stopWatch.Elapsed.TotalMilliseconds.ToString();
        }

        private void LoadState()
        {
            var stopWatch = Stopwatch.StartNew();

            SaveSystemSingleton.Instance.GameStateManager.LoadState();

            stopWatch.Stop();
            displayText.text = stopWatch.Elapsed.TotalMilliseconds.ToString();
        }

        private void WriteStateToStorage()
        {
            var stopWatch = Stopwatch.StartNew();

            SaveSystemSingleton.Instance.WriteStateToStorage(SaveFileName);

            stopWatch.Stop();
            displayText.text = stopWatch.Elapsed.TotalMilliseconds.ToString();
        }

        private void ReadStateFromStorage()
        {
            var stopWatch = Stopwatch.StartNew();

            SaveSystemSingleton.Instance.ReadStateFromStorage(SaveFileName);

            stopWatch.Stop();
            displayText.text = stopWatch.Elapsed.TotalMilliseconds.ToString();
            lastestSpeed = stopWatch.ElapsedMilliseconds;
        }

        private void MakeSaveablesDirty()
        {
            foreach (var item in gameObject.scene.GetRootGameObjects())
            {
                item.transform.Translate(Random.insideUnitSphere);
                item.transform.Rotate(Random.insideUnitSphere);
                item.transform.localScale = Random.insideUnitSphere;
            }
        }

        private void WipeSaveables()
        {
            var stopWatch = Stopwatch.StartNew();

            int saveableCount = saveables.Length;
            var gameStateManager = SaveSystemSingleton.Instance.GameStateManager;
            for (int i = 0; i < saveableCount; i++)
            {
                gameStateManager.WipeSaveable(saveables[i]);
            }

            stopWatch.Stop();
            displayText.text = stopWatch.Elapsed.TotalMilliseconds.ToString();
            lastestSpeed = stopWatch.ElapsedMilliseconds;
        }

        private void WipeSave()
        {
            SaveSystemSingleton.Instance.Storage.Remove(SaveFileName);
            SaveSystemSingleton.Instance.GameStateManager.GameState = new GameState();
            SaveSystemSingleton.Instance.GameStateManager.LoadState();
            displayText.text = "Save file deleted";
            lastestSpeed = 0;
        }
    }
}