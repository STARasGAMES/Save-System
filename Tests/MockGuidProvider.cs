using System;
using SaG.GuidReferences;
using UnityEngine;

namespace SaG.SaveSystem.Tests
{
    public class MockGuidProvider : MonoBehaviour, IGuidProvider
    {
        public string Guid { get; set; }

        public Guid GetGuid()
        {
            throw new NotImplementedException();
        }

        public string GetStringGuid()
        {
            return Guid;
        }
    }
}