using UnityEngine;

namespace SaG.SaveSystem.Samples.Platformer2D.UI
{
    public class LoadGameButton : MonoBehaviour
    {
        public void LoadGame()
        {
            GAME.LoadGameFromManualSave();
        }
    }
}