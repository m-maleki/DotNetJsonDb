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
                using (var writer = File.AppendText(_filePath))
                {
                    writer.WriteLine(JsonSerializer.Serialize(item));
                }
            }
            catch (Exception ex)
            {
                throw new JsonException($"Error saving data to {_filePath}: {ex.Message}");
            }
        }

        public T GetById(int id)
        {
            using (var reader = new StreamReader(_filePath))
            {
                while (reader.ReadLine() is { } line)
                {
                    var item = JsonSerializer.Deserialize<T>(line);
                    if ((int)_idProperty.GetValue(item) == id)
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        public List<T> GetAll()
        {
            var items = new List<T>();
            using (var reader = new StreamReader(_filePath))
            {
                while (reader.ReadLine() is { } line)
                {
                    items.Add(JsonSerializer.Deserialize<T>(line));
                }
            }

            return items;
        }

        public void Remove(int id)
        {
            var tempFile = Path.GetTempFileName();

            using (var reader = new StreamReader(_filePath))
            using (var writer = new StreamWriter(tempFile))
            {
                while (reader.ReadLine() is { } line)
                {
                    var item = JsonSerializer.Deserialize<T>(line);
                    if ((int)_idProperty.GetValue(item)! != id)
                    {
                        writer.WriteLine(line); 
                    }
                }
            }

            File.Delete(_filePath);
            File.Move(tempFile, _filePath);
        }

        public void Update(int id, T newItem)
        {
            var tempFile = Path.GetTempFileName();

            using (var reader = new StreamReader(_filePath))
            using (var writer = new StreamWriter(tempFile))
            {
                while (reader.ReadLine() is { } line)
                {
                    var item = JsonSerializer.Deserialize<T>(line);
                    writer.WriteLine((int)_idProperty.GetValue(item)! == id ? JsonSerializer.Serialize(newItem) : line);
                }
            }

            File.Delete(_filePath);
            File.Move(tempFile, _filePath);
        }
    }
}