using System;
using SaG.SaveSystem.Interfaces;
using UnityEngine;

namespace SaG.SaveSystem.Components
{
    /// <summary>
    /// Example class of how to store the visability of an object.
    /// This one took a bit longer, because of the edge-cases with scene loading and unloading
    /// </summary>
    [AddComponentMenu("Saving/Components/Save Visibility"), DisallowMultipleComponent]
    [SaveableComponentId("visibility")]
    public class SaveVisibility : MonoBehaviour, ISaveableComponent
    {
        private bool isEnabled;

        private void OnEnable()
        {
            isEnabled = true;
        }

        private void OnDisable()
        {
            // Ensure that it doesn't get toggled when the object is
            // deactivated /activated during scene load/unload
            if (SaveMaster.DeactivatedObjectExplicitly(gameObject))
            {
                isEnabled = false;
            }
        }

        public void Load(object data)
        {
            isEnabled = (bool) data;
            gameObject.SetActive(isEnabled);
        }

        public Type SaveDataType => typeof(bool);

        public object Save()
        {
            return isEnabled;
        }

        public bool OnSaveCondition()
        {
            return true;
        }
    }
}
