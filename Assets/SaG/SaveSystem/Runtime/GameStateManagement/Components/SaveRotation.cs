using System;
using UnityEngine;

namespace SaG.SaveSystem.GameStateManagement.Components
{
    /// <summary>
    /// Example class of how to store a rotation.
    /// Also very useful for people looking for a simple way to store a rotation.
    /// </summary>

    [AddComponentMenu("Saving/Components/Save Rotation"), DisallowMultipleComponent]
    [SaveableComponentId("rotation")]
    public class SaveRotation : MonoBehaviour, ISaveableComponent
    {
        private Quaternion lastRotation;

        public void Load(object data)
        {
            lastRotation = (Quaternion) data;
            transform.rotation = lastRotation;
        }

        public Type SaveDataType => typeof(Quaternion);

        public object Save()
        {
            lastRotation = transform.rotation;
            return lastRotation;
        }

        public bool OnSaveCondition()
        {
            return lastRotation != transform.rotation;
        }
    }
}
