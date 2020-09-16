using UnityEngine;

namespace SaG.SaveSystem.Samples.Platformer2D
{
    public class SceneTransitionTrigger : MonoBehaviour
    {
        [SerializeField] private string sceneName = default;
        [SerializeField] private Transform spawnPoint = default;
        

        public string SceneName => sceneName;

        public Vector3 SpawnPoint => spawnPoint.position;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                Game.LoadScene(sceneName);
            }
        }
    }
}