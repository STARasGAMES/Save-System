using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SaG.SaveSystem.Core
{
    public class SaveableContainerJObject : ISaveableContainer
    {
        private static JsonSerializer _serializer;
        private static JsonSerializer Serializer
        {
            get
            {
                if (_serializer == null)
                {
                    _serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    });
                }
                return _serializer;
            } 
        }
        public string Id { get; }

        public string Context { get; }

        private JObject _state;

        public SaveableContainerJObject(string id, string context = null)
        {
            Id = id;
            Context = context;
            _state = new JObject();
        }
        
        public void Load(JObject saveData)
        {
            if (saveData == null)
                throw new ArgumentNullException(nameof(saveData));
            _state = saveData;
        }

        public JObject Save()
        {
            if (_state == null)
                throw new NullReferenceException($"Trying to save null object. {nameof(_state)}");
            return _state;
        }
        
        public void Set<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(key);
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            _state[key] = JToken.FromObject(value, Serializer);
        }

        public T Get<T>(string key)
        {
            return (T)Get(key, typeof(T));
        }
        
        public object Get(string key, Type type)
        {
            if (_state.TryGetValue(key, out var value))
            {
                return value.ToObject(type);
            }
            throw new ArgumentOutOfRangeException(nameof(key), $"There is no such key: '{key}'.");
        }

        public bool TryGetValue(string key, out object value, Type type)
        {
            if (!_state.TryGetValue(key, out var d))
            {
                value = default;
                return false;
            }
            value = d.ToObject(type);
            return true;
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            var result = TryGetValue(key, out var d, typeof(T));
            value = (T)d;
            return result;
        }

        public bool Remove(string key)
        {
            return _state.Remove(key);
        }

        public void Clear()
        {
            _state.RemoveAll();
        }
    }
}