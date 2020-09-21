using System;
using UnityEngine;

namespace SaG.SaveSystem.GameStateManagement.Components
{
    /// <summary>
    /// Example class of how to store the scale of a gameObject.
    /// Also very useful for people looking for a simple way to store the scale.
    /// </summary>

    [AddComponentMenu("Saving/Components/Save Scale"), DisallowMultipleComponent]
    [SaveableComponentId("scale")]
    public class SaveScale : MonoBehaviour, ISaveableComponent
    {
        private Vector3 lastScale;

        public void Load(object data)
        {
            lastScale = (Vector3) data;
            transform.localScale = lastScale;
        }

        public Type SaveDataType => typeof(Vector3);

        public object Save()
        {
            lastScale = transform.localScale;
            return lastScale;
        }

        public bool OnSaveCondition()
        {
            return lastScale != transform.localScale;
        }
    }
}
