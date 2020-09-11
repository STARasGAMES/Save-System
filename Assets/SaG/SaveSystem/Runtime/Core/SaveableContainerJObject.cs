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
        
        private JObject _state;

        public SaveableContainerJObject(string id)
        {
            Id = id;
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

            return null;
        }

        public bool Remove(string key)
        {
            return _state.Remove(key);
        }
    }
}