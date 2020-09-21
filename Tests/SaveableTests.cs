using System;
using NUnit.Framework;
using SaG.GuidReferences;
using SaG.SaveSystem.GameStateManagement;
using UnityEngine;

namespace SaG.SaveSystem.Tests
{
    public class SaveableTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void Id_ReturnsGuidId()
        {
            var go = new GameObject("test saveable game object");
            go.SetActive(false);
            var saveable = go.AddComponent<Saveable>();
            var guidProvider = go.GetComponent<GuidComponent>();
            var guid = Guid.NewGuid();
            guidProvider.SetGuid(guid);
            Assert.AreEqual(guid.ToString(), saveable.Id);
        }
    }
}
