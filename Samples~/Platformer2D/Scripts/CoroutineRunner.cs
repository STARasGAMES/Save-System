using System.Collections;
using UnityEngine;

namespace SaG.SaveSystem.Samples.Platformer2D
{
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;

        public static CoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("CoroutineRunner");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<CoroutineRunner>();
                }

                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogError("WTF CoroutineRunner is already present.");
                Destroy(gameObject);
            }
        }

        public static Coroutine RunCoroutine(IEnumerator coroutine)
        {
            return Instance.StartCoroutine(coroutine);
        }
    }
}