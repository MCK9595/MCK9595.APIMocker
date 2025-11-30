namespace MCK9595.APIMocker.Core.Storage;

public record QueryOptions(
    Dictionary<string, string>? Filters = null,
    string? SortBy = null,
    bool SortDescending = false,
    int? Skip = null,
    int? Take = null
);

public record QueryResult(
    IReadOnlyList<Dictionary<string, object?>> Items,
    int TotalCount
);

public interface IDataStore
{
    IReadOnlyList<Dictionary<string, object?>> GetAll(string collection);
    QueryResult Query(string collection, QueryOptions options);
    Dictionary<string, object?>? GetById(string collection, object id);
    Dictionary<string, object?> Create(string collection, Dictionary<string, object?> item);
    Dictionary<string, object?>? Update(string collection, object id, Dictionary<string, object?> item);
    bool Delete(string collection, object id);
    void Seed(string collection, IEnumerable<Dictionary<string, object?>> items);
    int GetNextId(string collection);
}
