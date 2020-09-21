using System;

namespace SaG.SaveSystem
{
    public interface IContainer
    {
        void Set<T>(string key, T value);

        T Get<T>(string key);

        object Get(string key, Type type);

        bool TryGetValue(string key, out object value, Type type);

        bool TryGetValue<T>(string key, out T value);

        bool ContainsKey(string key);

        bool Remove(string key);

        void Clear();
    }
}