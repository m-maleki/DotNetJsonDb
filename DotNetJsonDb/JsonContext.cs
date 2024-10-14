using System.Reflection;
using System.Text.Json;

namespace DotNetJsonDb
{
    public class JsonContext<T> where T : class
    {
        private readonly string _filePath;
        private readonly PropertyInfo _idProperty;

        public JsonContext(string basePath = "Database")
        {
            _filePath = Path.Combine(basePath, typeof(T).Name + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));

            _idProperty = typeof(T).GetProperties()
                .FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) &&
                                     p.PropertyType == typeof(int))!;

            if (_idProperty == null)
            {
                throw new InvalidOperationException("Type T must have an integer property named 'Id'.");
            }
        }

        public void Add(T item)
        {
            try
            {
                List<T> items = new List<T>();
                if (File.Exists(_filePath))
                {
                    using (StreamReader reader = new StreamReader(_filePath))
                    {
                        string json = reader.ReadToEnd();

                        if (!string.IsNullOrEmpty(json) && json.Trim().StartsWith("[") && json.Trim().EndsWith("]"))
                        {
                            try
                            {
                                items = JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
                            }
                            catch (JsonException)
                            {
                                items = new List<T>();
                            }
                        }
                    }
                }

                items.Add(item);

                string newJson = JsonSerializer.Serialize(items);

                using (FileStream fs = new FileStream(_filePath, FileMode.Create))
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.Write(newJson);
                }
            }
            catch (Exception ex)
            {
                throw new JsonException($"Error saving data to {_filePath}: {ex.Message}");
            }
        }

        public T Get(int id)
        {
            try
            {
                using (var reader = new StreamReader(_filePath))
                {
                    string json = reader.ReadToEnd();
                    var items = JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();

                    return items.FirstOrDefault(item => (int)_idProperty.GetValue(item) == id);
                }
            }
            catch (Exception ex)
            {
                throw new JsonException($"Error getting data from {_filePath}: {ex.Message}");
            }
        }

        public List<T> GetAll()
        {
            try
            {
                using (var reader = new StreamReader(_filePath))
                {
                    string json = reader.ReadToEnd();
                    return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
                }
            }
            catch (Exception ex)
            {
                throw new JsonException($"Error reading data from {_filePath}: {ex.Message}");
            }
        }

        public void Remove(int id)
        {
            try
            {
                using (var reader = new StreamReader(_filePath))
                {
                    string json = reader.ReadToEnd();
                    var items = JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();

                    items.RemoveAll(item => (int)_idProperty.GetValue(item)! == id);

                    reader.Close();

                    string newJson = JsonSerializer.Serialize(items);

                    File.WriteAllText(_filePath, newJson);
                }
            }
            catch (Exception ex)
            {
                throw new JsonException($"Error removing data from {_filePath}: {ex.Message}");
            }
        }

        public void Update(int id, T newItem)
        {
            try
            {
                using (var reader = new StreamReader(_filePath))
                {
                    string json = reader.ReadToEnd();
                    var items = JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();

                    int index = items.FindIndex(item => (int)_idProperty.GetValue(item)! == id);
                    if (index != -1)
                    {
                        items[index] = newItem;
                    }

                    string newJson = JsonSerializer.Serialize(items);

                    reader.Close();

                    File.WriteAllText(_filePath, newJson);
                }
            }
            catch (Exception ex)
            {
                throw new JsonException($"Error updating data in {_filePath}: {ex.Message}");
            }
        }


    }
}