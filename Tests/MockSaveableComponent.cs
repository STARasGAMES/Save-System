using System;
using SaG.SaveSystem.Interfaces;
using UnityEngine;

namespace SaG.SaveSystem.Tests
{
    public class MockSaveableComponent : MonoBehaviour, ISaveableComponent
    {
        public object MockSaveData { get; set; }
        public object LoadedSaveData { get; private set; }
        public Type SaveDataType => MockSaveData.GetType();
        public object Save()
        {
            return MockSaveData;
        }

        public void Load(object data)
        {
            LoadedSaveData = data;
        }

        public bool OnSaveCondition()
        {
            return true;
        }
    }
}