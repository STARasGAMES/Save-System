using System;
using Newtonsoft.Json.Linq;

namespace SaG.SaveSystem.Interfaces
{
    public interface ISaveableContainer
    {
        string Id { get; }

        JObject Save();

        void Load(JObject state);
        
        void Set<T>(string key, T value);

        T Get<T>(string key);

        object Get(string key, Type type);

        bool Remove(string key);
    }
}