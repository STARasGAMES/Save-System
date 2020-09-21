using System;
using SaG.GuidReferences;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace SaG.SaveSystem.Samples.Platformer2D.CustomGuidProvider
{
    [CreateAssetMenu(fileName = "new Scriptable Guid Provider",
        menuName = "SaveSystem/Samples/Platformer2D/Scriptable Guid Provider", order = 0)]
    public class ScriptableObjectGuidProviderAsset : ScriptableObject, ISerializationCallbackReceiver, IGuidProvider
    {
        // System guid we use for comparison and generation
        Guid guid = Guid.Empty;
        private string cachedStringGuid;

        // Unity's serialization system doesn't know about System.Guid, so we convert to a byte array
        // Fun fact, we tried using strings at first, but that allocated memory and was twice as slow
        [SerializeField] private byte[] serializedGuid;

        public bool IsGuidAssigned()
        {
            return guid != System.Guid.Empty;
        }


        // When de-serializing or creating this component, we want to either restore our serialized GUID
        // or create a new one.
        void CreateGuid()
        {
            // if our serialized data is invalid, then we are a new object and need a new GUID
            if (serializedGuid == null || serializedGuid.Length != 16)
            {
#if UNITY_EDITOR
                Undo.RecordObject(this, "Added GUID");
#endif
                guid = Guid.NewGuid();
                serializedGuid = guid.ToByteArray();
            }
            else if (guid == Guid.Empty)
            {
                // otherwise, we should set our system guid to our serialized guid
                guid = new Guid(serializedGuid);
            }
        }

        // We cannot allow a GUID to be saved into a prefab, and we need to convert to byte[]
        public void OnBeforeSerialize()
        {
            if (guid != System.Guid.Empty)
            {
                serializedGuid = guid.ToByteArray();
            }
        }

        // On load, we can go head a restore our system guid for later use
        public void OnAfterDeserialize()
        {
            if (serializedGuid != null && serializedGuid.Length == 16)
            {
                guid = new System.Guid(serializedGuid);
            }
        }

        void Awake()
        {
            CreateGuid();
        }

        void OnValidate()
        {
            CreateGuid();
        }

        // Never return an invalid GUID
        public Guid GetGuid()
        {
            if (guid == Guid.Empty && serializedGuid != null && serializedGuid.Length == 16)
            {
                guid = new Guid(serializedGuid);
            }

            return guid;
        }

        public string GetStringGuid()
        {
            if (cachedStringGuid == null)
            {
                cachedStringGuid = GetGuid().ToString();
            }

            return cachedStringGuid;
        }

        /// <summary>
        /// Generates new Guid. Be careful with this method. It should be called only in specific cases.
        /// </summary>
        public void RegenerateGuid()
        {
            GuidManagerSingleton.Remove(guid);
            serializedGuid = null;
            guid = System.Guid.Empty;
            cachedStringGuid = null;
            CreateGuid();
        }
    }
}