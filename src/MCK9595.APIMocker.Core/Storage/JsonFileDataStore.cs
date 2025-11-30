using System.Text.Json;

namespace MCK9595.APIMocker.Core.Storage;

/// <summary>
/// JSON file-based data store with persistence support.
/// Data is cached in memory and persisted to JSON files on mutations.
/// </summary>
public class JsonFileDataStore : IDataStore
{
    private readonly string _dataDirectory;
    private readonly InMemoryDataStore _cache = new();
    private readonly object _fileLock = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonFileDataStore(string dataDirectory)
    {
        _dataDirectory = dataDirectory;
        Directory.CreateDirectory(_dataDirectory);
        LoadAllCollections();
    }

    private void LoadAllCollections()
    {
        if (!Directory.Exists(_dataDirectory)) return;

        foreach (var file in Directory.GetFiles(_dataDirectory, "*.json"))
        {
            var collection = Path.GetFileNameWithoutExtension(file);
            LoadCollection(collection);
        }
    }

    private void LoadCollection(string collection)
    {
        var filePath = GetFilePath(collection);
        if (!File.Exists(filePath)) return;

        try
        {
            var json = File.ReadAllText(filePath);
            var items = JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(json, _jsonOptions);
            if (items != null)
            {
                foreach (var item in items)
                {
                    // Convert JsonElement values to proper types
                    var converted = ConvertJsonElements(item);
                    _cache.Create(collection, converted);
                }
            }
        }
        catch (JsonException)
        {
            // Invalid JSON, ignore and start fresh
        }
    }

    private Dictionary<string, object?> ConvertJsonElements(Dictionary<string, object?> item)
    {
        var result = new Dictionary<string, object?>();
        foreach (var (key, value) in item)
        {
            result[key] = ConvertValue(value);
        }
        return result;
    }

    private object? ConvertValue(object? value)
    {
        if (value is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt32(out var i) ? i : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Array => element.EnumerateArray()
                    .Select(e => ConvertValue(e))
                    .ToList(),
                JsonValueKind.Object => element.EnumerateObject()
                    .ToDictionary(p => p.Name, p => ConvertValue(p.Value)),
                _ => value
            };
        }
        return value;
    }

    private void SaveCollection(string collection)
    {
        lock (_fileLock)
        {
            var items = _cache.GetAll(collection);
            var json = JsonSerializer.Serialize(items, _jsonOptions);
            var filePath = GetFilePath(collection);
            File.WriteAllText(filePath, json);
        }
    }

    private string GetFilePath(string collection)
    {
        return Path.Combine(_dataDirectory, $"{collection}.json");
    }

    public IReadOnlyList<Dictionary<string, object?>> GetAll(string collection)
    {
        return _cache.GetAll(collection);
    }

    public QueryResult Query(string collection, QueryOptions options)
    {
        return _cache.Query(collection, options);
    }

    public Dictionary<string, object?>? GetById(string collection, object id)
    {
        return _cache.GetById(collection, id);
    }

    public Dictionary<string, object?> Create(string collection, Dictionary<string, object?> item)
    {
        var created = _cache.Create(collection, item);
        SaveCollection(collection);
        return created;
    }

    public Dictionary<string, object?>? Update(string collection, object id, Dictionary<string, object?> item)
    {
        var updated = _cache.Update(collection, id, item);
        if (updated != null)
        {
            SaveCollection(collection);
        }
        return updated;
    }

    public bool Delete(string collection, object id)
    {
        var deleted = _cache.Delete(collection, id);
        if (deleted)
        {
            SaveCollection(collection);
        }
        return deleted;
    }

    public void Seed(string collection, IEnumerable<Dictionary<string, object?>> items)
    {
        _cache.Seed(collection, items);
        SaveCollection(collection);
    }

    public int GetNextId(string collection)
    {
        return _cache.GetNextId(collection);
    }

    /// <summary>
    /// Load seed data from a JSON file containing multiple collections.
    /// Format: { "users": [...], "products": [...] }
    /// </summary>
    public void LoadSeedFile(string seedFilePath)
    {
        if (!File.Exists(seedFilePath))
        {
            throw new FileNotFoundException($"Seed file not found: {seedFilePath}");
        }

        var json = File.ReadAllText(seedFilePath);
        var data = JsonSerializer.Deserialize<Dictionary<string, List<Dictionary<string, object?>>>>(json, _jsonOptions);

        if (data == null) return;

        foreach (var (collection, items) in data)
        {
            foreach (var item in items)
            {
                var converted = ConvertJsonElements(item);
                _cache.Create(collection, converted);
            }
            SaveCollection(collection);
        }
    }
}
