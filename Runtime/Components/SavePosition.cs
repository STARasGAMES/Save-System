using System;
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
        Vector3 _lastPosition;

        public void Load(object data)
        {
            var pos = (Vector3)data;
            transform.position = pos;
            _lastPosition = pos;
        }

        public Type SaveDataType => typeof(Vector3);

        public object Save()
        {
            _lastPosition = transform.position;
            return _lastPosition;
        }

        public bool OnSaveCondition()
        {
            return _lastPosition != transform.position;
        }
    }
}
