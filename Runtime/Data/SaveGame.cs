using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaG.SaveSystem.Data
{
    /// <summary>
    /// Container for all saved data.
    /// Placed into a slot (separate save file)
    /// </summary>
    [Serializable]
    public class SaveGame
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
        [SerializeField] public Dictionary<string, Data> saveData = new Dictionary<string, Data>(StringComparer.Ordinal);

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

        /// <summary>
        /// Wipes all data that associates with provided sceneName. Returns count of removed records.
        /// </summary>
        /// <param name="sceneName">Scene name</param>
        /// <returns>Returns count of removed records.</returns>
        public int WipeSceneData(string sceneName)
        {
            List<string> keysToRemove = new List<string>();
            foreach (var data in saveData)
            {
                if (data.Value.context == sceneName)
                    keysToRemove.Add(data.Key);
            }

            foreach (var key in keysToRemove)
            {
                Remove(key);
            }

            return keysToRemove.Count;
        }

        /// <summary>
        /// Removes record by its id. Returns <c>true</c> if removed something, otherwise, <c>false</c>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns <c>true</c> if removed something, otherwise, <c>false</c>.</returns>
        public bool Remove(string id)
        {
            return saveData.Remove(id);
        }

        /// <summary>
        /// Assign any data to the given ID. If data is already present within the ID, then it will be overwritten.
        /// </summary>
        /// <param name="id"> Save Identification </param>
        /// <param name="data"> Data in a string format </param>
        /// <param name="sceneName">Scene name</param>
        public void Set(string id, object data, string sceneName)
        {
            saveData[id] = new Data(){data = data, context = sceneName};
        }

        public bool ContainsId(string id) => saveData.ContainsKey(id);

        public bool TryGetValue(string id, out object value)
        {
            if (!saveData.TryGetValue(id, out var data))
            {
                value = default;
                return false;
            }
            value = data.data;
            return true;
        }

        /// <summary>
        /// Returns any data stored based on a identifier
        /// </summary>
        /// <param name="id"> Save Identification </param>
        /// <returns></returns>
        public object Get(string id)
        {
            if (saveData.TryGetValue(id, out var data))
                return data.data;

            throw new ArgumentOutOfRangeException(nameof(id), $"There is no data with id: {id}");
        }
    }
}