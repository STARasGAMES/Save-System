using System;
using SaG.SaveSystem.Interfaces;
using UnityEngine;

namespace SaG.SaveSystem.Components
{
    /// <summary>
    /// Example class of how to store a position.
    /// Also very useful for people looking for a simple way to store a position.
    /// </summary>

    [AddComponentMenu("Saving/Components/Save Position"), DisallowMultipleComponent]
    [SaveableComponentId("position")]
    public class SavePosition : MonoBehaviour, ISaveableComponent
    {
        Vector3 lastPosition;

        [Serializable]
        private struct SaveData
        {
            public Vector3 position;
        }

        public void Load(object data)
        {
            var pos = ((SaveData)data).position;
            transform.position = pos;
            lastPosition = pos;
        }

        public Type SaveDataType => typeof(SaveData);

        public object Save()
        {
            lastPosition = transform.position;
            return new SaveData { position = lastPosition };
        }

        public bool OnSaveCondition()
        {
            return lastPosition != transform.position;
        }
    }
}
