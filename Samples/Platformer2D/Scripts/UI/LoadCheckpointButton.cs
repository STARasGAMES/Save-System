using UnityEngine;

namespace SaG.SaveSystem.Samples.Platformer2D.UI
{
    public class LoadCheckpointButton : MonoBehaviour
    {
        public void LoadCheckpoint()
        {
            Game.LoadGameFromFile("checkpoint");
        }
    }
}