using System;
using SaG.SaveSystem.Components;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SaG.SaveSystem.Samples.RuntimeSpawn
{
    [SaveableComponentId("sample_saveable")]
    public class SamplePrefabSaveableComponent : MonoBehaviour, ISaveableComponent
    {
        [SerializeField] private Renderer _renderer = default;
        [SerializeField] private Gradient _colors = default;

        private bool _isLoaded = false;

        private void Awake()
        {
            if (!_isLoaded)
                _renderer.material.color = _colors.Evaluate(Random.Range(0f, 1f));
        }

        private class SaveData
        {
            public Color color;
        }

        public Type SaveDataType => typeof(SaveData);

        public object Save()
        {
            return new SaveData()
            {
                color = _renderer.sharedMaterial.color
            };
        }

        public void Load(object data)
        {
            _isLoaded = true;
            var saveData = (SaveData) data;
            _renderer.material.color = saveData.color;
        }

        public bool OnSaveCondition()
        {
            return true;
        }
    }
}