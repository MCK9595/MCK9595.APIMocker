using MCK9595.APIMocker.Core.Storage;
using Xunit;

namespace MCK9595.APIMocker.Core.Tests;

public class JsonFileDataStoreTests : IDisposable
{
    private readonly string _testDir;
    private JsonFileDataStore? _store;

    public JsonFileDataStoreTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"mck-test-{Guid.NewGuid()}");
    }

    public void Dispose()
    {
        _store = null;
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, recursive: true);
        }
    }

    [Fact]
    public void Create_PersistsToFile()
    {
        _store = new JsonFileDataStore(_testDir);

        _store.Create("users", new Dictionary<string, object?> { ["name"] = "Test" });

        var filePath = Path.Combine(_testDir, "users.json");
        Assert.True(File.Exists(filePath));

        var content = File.ReadAllText(filePath);
        Assert.Contains("Test", content);
    }

    [Fact]
    public void Update_PersistsToFile()
    {
        _store = new JsonFileDataStore(_testDir);
        var created = _store.Create("users", new Dictionary<string, object?> { ["name"] = "Original" });

        _store.Update("users", created["id"]!, new Dictionary<string, object?> { ["name"] = "Updated" });

        var content = File.ReadAllText(Path.Combine(_testDir, "users.json"));
        Assert.Contains("Updated", content);
        Assert.DoesNotContain("Original", content);
    }

    [Fact]
    public void Delete_PersistsToFile()
    {
        _store = new JsonFileDataStore(_testDir);
        var created = _store.Create("users", new Dictionary<string, object?> { ["name"] = "ToDelete" });

        _store.Delete("users", created["id"]!);

        var content = File.ReadAllText(Path.Combine(_testDir, "users.json"));
        Assert.DoesNotContain("ToDelete", content);
    }

    [Fact]
    public void LoadsExistingData_OnConstruction()
    {
        // Setup: Create store and add data
        _store = new JsonFileDataStore(_testDir);
        _store.Create("users", new Dictionary<string, object?> { ["name"] = "Existing" });
        _store = null;

        // Act: Create new store pointing to same directory
        var newStore = new JsonFileDataStore(_testDir);

        // Assert: Data is loaded
        var items = newStore.GetAll("users");
        Assert.Single(items);
        Assert.Equal("Existing", items[0]["name"]?.ToString());
    }

    [Fact]
    public void LoadSeedFile_LoadsMultipleCollections()
    {
        var seedPath = Path.Combine(_testDir, "seed.json");
        Directory.CreateDirectory(_testDir);
        File.WriteAllText(seedPath, @"{
            ""users"": [{ ""name"": ""User1"" }],
            ""products"": [{ ""name"": ""Product1"" }]
        }");

        _store = new JsonFileDataStore(_testDir);
        _store.LoadSeedFile(seedPath);

        Assert.Single(_store.GetAll("users"));
        Assert.Single(_store.GetAll("products"));
    }

    [Fact]
    public void LoadSeedFile_ThrowsIfNotFound()
    {
        _store = new JsonFileDataStore(_testDir);

        Assert.Throws<FileNotFoundException>(() => _store.LoadSeedFile("/nonexistent.json"));
    }

    [Fact]
    public void Query_WorksWithPersistedData()
    {
        _store = new JsonFileDataStore(_testDir);
        _store.Create("users", new Dictionary<string, object?> { ["name"] = "Alice", ["role"] = "admin" });
        _store.Create("users", new Dictionary<string, object?> { ["name"] = "Bob", ["role"] = "user" });

        var result = _store.Query("users", new QueryOptions(
            Filters: new Dictionary<string, string> { ["role"] = "admin" }
        ));

        Assert.Single(result.Items);
        Assert.Equal("Alice", result.Items[0]["name"]);
    }
}
