using UnityEngine;

namespace SaG.SaveSystem.Samples.Platformer2D.UI
{
    public class ManualSaveGameButton : MonoBehaviour
    {
        public void SaveGame()
        {
            Game.ManualSave();
        }
    }
}