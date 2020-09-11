using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SaG.SaveSystem.Data
{
    /// <summary>
    /// Container for all saved data.
    /// Placed into a slot (separate save file)
    /// </summary>
    [Serializable]
    public class GameState : IGameState
    {
        [Serializable]
        public struct MetaData
        {
            public int gameVersion;
            public string creationDate;
            public string timePlayed;
        }

        [Serializable]
        public struct Data
        {
            public string context;
            public object data;
        }

        [NonSerialized] public TimeSpan timePlayed;
        [NonSerialized] public int gameVersion;
        [NonSerialized] public DateTime creationDate;

        [SerializeField] public MetaData metaData;
        [SerializeField] public Dictionary<string, Data> stateData = new Dictionary<string, Data>(StringComparer.Ordinal);

        [NonSerialized] private bool loaded;

        public void OnWrite()
        {
            if (creationDate == default(DateTime))
            {
                creationDate = DateTime.Now;
            }

            metaData.creationDate = creationDate.ToString();
            metaData.gameVersion = gameVersion;
            metaData.timePlayed = timePlayed.ToString();
        }

        public void OnLoad()
        {
            gameVersion = metaData.gameVersion;

            DateTime.TryParse(metaData.creationDate, out creationDate);
            TimeSpan.TryParse(metaData.timePlayed, out timePlayed);
            // todo maybe clear save data from null objects
        }

        
        /// <inheritdoc/>
        public bool Remove(string id)
        {
            return stateData.Remove(id);
        }
        
        /// <inheritdoc/>
        public int RemoveContext(string context)
        {
            var keysToRemove = stateData.Where(pair => pair.Value.context == context).Select(pair => pair.Key).ToList();

            foreach (var key in keysToRemove)
            {
                Remove(key);
            }

            return keysToRemove.Count;
        }

        /// <inheritdoc/>
        public void Set(string id, object value, string context)
        {
            stateData[id] = new Data(){data = value, context = context};
        }
        
        /// <inheritdoc/>
        public bool ContainsId(string id) => stateData.ContainsKey(id);
        
        /// <inheritdoc/>
        public bool TryGetValue(string id, out object value)
        {
            if (!stateData.TryGetValue(id, out var data))
            {
                value = default;
                return false;
            }
            value = data.data;
            return true;
        }

        /// <inheritdoc/>
        public object Get(string id)
        {
            if (stateData.TryGetValue(id, out var data))
                return data.data;

            throw new ArgumentOutOfRangeException(nameof(id), $"There is no data with id: {id}");
        }
    }
}