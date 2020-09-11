using System;
using System.Collections;
using NUnit.Framework;
using SaG.GuidReferences;
using SaG.SaveSystem.Components;
using SaG.SaveSystem.Core;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace SaG.SaveSystem.Tests
{
    public class SaveMasterTests
    {
        [SetUp]
        public void SetUp()
        {
            if (SaveMaster.IsSlotUsed(46))
                SaveMaster.DeleteSave(46);
            if (SaveMaster.IsSlotUsed(47))
                SaveMaster.DeleteSave(47);
            SaveMaster.SetSlot(47, false);
        }

        [TearDown]
        public void TearDownScene()
        {
            SaveMaster.WipeSceneData("Global");
            SaveMaster.WipeSceneData(Object.FindObjectOfType<Transform>().gameObject.scene.name);
            if (SaveMaster.IsSlotUsed(46))
                SaveMaster.DeleteSave(46);
            if (SaveMaster.IsSlotUsed(47))
                SaveMaster.DeleteSave(47);
        }

        [Test]
        public void SavePrimitiveValues_String_Float_Int()
        {
            SaveMaster.SetString("test_string_id", "test_string_value");
            SaveMaster.SetFloat("test_float_id", 47.47f);
            SaveMaster.SetInt("test_int_id", 47);
            SaveMaster.SyncSave();
            SaveMaster.WriteActiveSaveToDisk();
            Assert.AreEqual("test_string_value", SaveMaster.GetString("test_string_id"));
            Assert.AreEqual(47.47f, SaveMaster.GetFloat("test_float_id"));
            Assert.AreEqual(47, SaveMaster.GetInt("test_int_id"));
            SaveMaster.SetString("test_string_id", "test_string_value_another");
            SaveMaster.SetFloat("test_float_id", 48.48f);
            SaveMaster.SetInt("test_int_id", 48);
            Assert.AreEqual("test_string_value_another", SaveMaster.GetString("test_string_id"));
            Assert.AreEqual(48.48f, SaveMaster.GetFloat("test_float_id"));
            Assert.AreEqual(48, SaveMaster.GetInt("test_int_id"));
            SaveMaster.LoadActiveSaveFromDisk(true);
            Assert.AreEqual("test_string_value", SaveMaster.GetString("test_string_id"));
            Assert.AreEqual(47.47f, SaveMaster.GetFloat("test_float_id"));
            Assert.AreEqual(47, SaveMaster.GetInt("test_int_id"));
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator SaveMasterTest_Save_and_Load()
        {
            var go = new GameObject("test saveable object");
            go.SetActive(false);
            var saveable = go.AddComponent<Saveable>();
            var guidProvider = go.GetComponent<GuidComponent>();
            var guid = Guid.NewGuid();
            guidProvider.SetGuid(guid);
            var mockSaveableComponent = go.AddComponent<MockSaveableComponent>();
            mockSaveableComponent.MockSaveData = new MockSaveData {someData = "DATA"};
            saveable.AddSaveableComponent("mock_component_id", mockSaveableComponent, false);
            go.SetActive(true);
            SaveMaster.SyncSave();
            SaveMaster.WriteActiveSaveToDisk();
            yield return null;
            Assert.IsNull(mockSaveableComponent.LoadedSaveData);
            SaveMaster.SyncLoad();
            Assert.AreEqual(((MockSaveData) mockSaveableComponent.MockSaveData).someData,
                ((MockSaveData) mockSaveableComponent.LoadedSaveData).someData);
        }

        private class MockSaveData
        {
            public string someData;
        }
    }
}