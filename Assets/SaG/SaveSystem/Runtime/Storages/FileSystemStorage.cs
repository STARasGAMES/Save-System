using System;

namespace SaG.SaveSystem.Storages
{
    public class FileSystemStorage : IStorage
    {
        private readonly FileUtility _fileUtility;

        public FileSystemStorage()
        {
            _fileUtility = new FileUtility();
        }
        
        public void Set<T>(string key, T value)
        {
            _fileUtility.WriteObjectToFile(key, value);
        }

        public T Get<T>(string key)
        {
            return _fileUtility.ReadObjectFromFile<T>(key);
        }

        public object Get(string key, Type type)
        {
            return _fileUtility.ReadObjectFromFile(key, type);
        }

        public bool TryGetValue(string key, out object value, Type type)
        {
            if (!_fileUtility.IsFileExist(key))
            {
                value = default;
                return false;
            }

            value = Get(key, type);
            return true;
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            if (!_fileUtility.IsFileExist(key))
            {
                value = default;
                return false;
            }

            value = Get<T>(key);
            return true;
        }

        public bool ContainsKey(string key)
        {
            return _fileUtility.IsFileExist(key);
        }

        public bool Remove(string key)
        {
            _fileUtility.DeleteFile(key);
            return true;
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }
    }
}