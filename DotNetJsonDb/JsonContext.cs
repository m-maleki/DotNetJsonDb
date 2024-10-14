using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace DotNetJsonDb
{
    public class JsonContext<T> where T : class
    {
        private readonly string _filePath;
        private readonly PropertyInfo _idProperty;

        public JsonContext(string basePath)
        {
            _filePath = Path.Combine(basePath, typeof(T).Name + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));

            // Find the "Id" property using Reflection during construction
            _idProperty = typeof(T).GetProperties()
                .FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) &&
                                     p.PropertyType == typeof(int));

            if (_idProperty == null)
            {
                throw new InvalidOperationException("Type T must have an integer property named 'Id'.");
            }
        }

        public void Add(T item)
        {
            var existingItems = LoadData();
            existingItems.Add(item);
            SaveData(existingItems);
        }

        public T GetById(int id)
        {
            var items = LoadData();
            return items.FirstOrDefault(item => (int)_idProperty.GetValue(item) == id);
        }

        public List<T> GetAll()
        {
            return LoadData();
        }

        public void Remove(int id)
        {
            var items = LoadData();
            items.RemoveAll(item => (int)_idProperty.GetValue(item) == id);
            SaveData(items);
        }

        public void Update(int id, T newItem)
        {
            var items = LoadData();
            var index = items.FindIndex(item => (int)_idProperty.GetValue(item) == id);

            if (index >= 0)
            {
                items[index] = newItem;
                SaveData(items);
            }
            else
            {
                throw new KeyNotFoundException($"Item with ID {id} not found.");
            }
        }

        private List<T> LoadData()
        {
            if (!File.Exists(_filePath))
            {
                return new List<T>();
            }

            try
            {
                using (var stream = File.OpenRead(_filePath))
                {
                    return JsonSerializer.Deserialize<List<T>>(stream);
                }
            }
            catch (Exception ex)
            {
                throw new JsonException($"Error loading data from {_filePath}: {ex.Message}");
            }
        }

        private void SaveData(List<T> data)
        {
            try
            {
                using (var stream = File.OpenWrite(_filePath))
                {
                    JsonSerializer.Serialize(stream, data, new JsonSerializerOptions { WriteIndented = true });
                }
            }
            catch (Exception ex)
            {
                throw new JsonException($"Error saving data to {_filePath}: {ex.Message}");
            }
        }
    }
}