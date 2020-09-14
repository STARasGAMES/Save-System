using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace SaG.SaveSystem.Core
{
    public class FileUtility
    {
        public string Path => Application.persistentDataPath + "/";

        public string GetPathForFile(string fileName) => $"{Path}{fileName}";
        
        public JsonSerializerSettings JsonSerializerSettings { get; set; } = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
        };
        
        public void WriteTextToFile(string fileName, string content)
        {
            // todo encoding and cryptography
            File.WriteAllText(GetPathForFile(fileName), content);
        }
        
        public string ReadTextFromFile(string fileName)
        {
            // todo encoding and cryptography
            return File.ReadAllText(GetPathForFile(fileName));
        }

        public void WriteObjectToFile(string fileName, object @object)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));
            if (@object == null)
                throw new ArgumentNullException(nameof(@object));
            string serializedObject = JsonConvert.SerializeObject(@object, JsonSerializerSettings);
            WriteTextToFile(fileName, serializedObject);
        }

        public T ReadObjectFromFile<T>(string fileName)
        {
            string serializedObject = ReadTextFromFile(fileName);
            return JsonConvert.DeserializeObject<T>(serializedObject, JsonSerializerSettings);
        }
        
        public object ReadObjectFromFile(string fileName, Type type)
        {
            string serializedObject = ReadTextFromFile(fileName);
            return JsonConvert.DeserializeObject(serializedObject, type, JsonSerializerSettings);
        }

        public bool IsFileExist(string fileName)
        {
            return File.Exists(GetPathForFile(fileName));
        }

        public void DeleteFile(string fileName)
        {
            File.Delete(fileName);
        }
    }
}