namespace MCK9595.APIMocker.Core.Storage;

public class InMemoryDataStore : IDataStore
{
    private readonly Dictionary<string, List<Dictionary<string, object?>>> _data = new();
    private readonly Dictionary<string, int> _nextIds = new();
    private readonly object _lock = new();

    public IReadOnlyList<Dictionary<string, object?>> GetAll(string collection)
    {
        lock (_lock)
        {
            if (!_data.TryGetValue(collection, out var items))
            {
                return Array.Empty<Dictionary<string, object?>>();
            }
            return items.ToList();
        }
    }

    public QueryResult Query(string collection, QueryOptions options)
    {
        lock (_lock)
        {
            if (!_data.TryGetValue(collection, out var items))
            {
                return new QueryResult(Array.Empty<Dictionary<string, object?>>(), 0);
            }

            IEnumerable<Dictionary<string, object?>> result = items;

            // Apply filters
            if (options.Filters != null)
            {
                foreach (var (field, value) in options.Filters)
                {
                    result = result.Where(item =>
                        item.TryGetValue(field, out var itemValue) &&
                        MatchesFilter(itemValue, value));
                }
            }

            // Get total count before pagination
            var filteredList = result.ToList();
            var totalCount = filteredList.Count;

            // Apply sorting
            if (!string.IsNullOrEmpty(options.SortBy))
            {
                filteredList = options.SortDescending
                    ? filteredList.OrderByDescending(item => GetSortValue(item, options.SortBy)).ToList()
                    : filteredList.OrderBy(item => GetSortValue(item, options.SortBy)).ToList();
            }

            // Apply pagination
            IEnumerable<Dictionary<string, object?>> pagedResult = filteredList;
            if (options.Skip.HasValue)
            {
                pagedResult = pagedResult.Skip(options.Skip.Value);
            }
            if (options.Take.HasValue)
            {
                pagedResult = pagedResult.Take(options.Take.Value);
            }

            return new QueryResult(pagedResult.ToList(), totalCount);
        }
    }

    private static bool MatchesFilter(object? itemValue, string filterValue)
    {
        if (itemValue == null)
        {
            return string.IsNullOrEmpty(filterValue);
        }

        var itemString = itemValue.ToString() ?? "";

        // Case-insensitive contains match
        return itemString.Contains(filterValue, StringComparison.OrdinalIgnoreCase);
    }

    private static object? GetSortValue(Dictionary<string, object?> item, string sortBy)
    {
        return item.TryGetValue(sortBy, out var value) ? value : null;
    }

    public Dictionary<string, object?>? GetById(string collection, object id)
    {
        lock (_lock)
        {
            if (!_data.TryGetValue(collection, out var items))
            {
                return null;
            }

            return items.FirstOrDefault(item =>
                item.TryGetValue("id", out var itemId) &&
                itemId?.ToString() == id.ToString());
        }
    }

    public Dictionary<string, object?> Create(string collection, Dictionary<string, object?> item)
    {
        lock (_lock)
        {
            EnsureCollection(collection);

            // Auto-assign ID if not present
            if (!item.ContainsKey("id") || item["id"] == null)
            {
                item["id"] = GetNextIdInternal(collection);
            }
            else
            {
                // Update next ID if necessary
                if (int.TryParse(item["id"]?.ToString(), out var itemId))
                {
                    if (!_nextIds.ContainsKey(collection))
                    {
                        _nextIds[collection] = 1;
                    }
                    if (itemId >= _nextIds[collection])
                    {
                        _nextIds[collection] = itemId + 1;
                    }
                }
            }

            _data[collection].Add(item);
            return item;
        }
    }

    public Dictionary<string, object?>? Update(string collection, object id, Dictionary<string, object?> item)
    {
        lock (_lock)
        {
            if (!_data.TryGetValue(collection, out var items))
            {
                return null;
            }

            var index = items.FindIndex(i =>
                i.TryGetValue("id", out var itemId) &&
                itemId?.ToString() == id.ToString());

            if (index == -1)
            {
                return null;
            }

            // Preserve ID
            item["id"] = items[index]["id"];
            items[index] = item;
            return item;
        }
    }

    public bool Delete(string collection, object id)
    {
        lock (_lock)
        {
            if (!_data.TryGetValue(collection, out var items))
            {
                return false;
            }

            var index = items.FindIndex(i =>
                i.TryGetValue("id", out var itemId) &&
                itemId?.ToString() == id.ToString());

            if (index == -1)
            {
                return false;
            }

            items.RemoveAt(index);
            return true;
        }
    }

    public void Seed(string collection, IEnumerable<Dictionary<string, object?>> items)
    {
        lock (_lock)
        {
            EnsureCollection(collection);
            foreach (var item in items)
            {
                Create(collection, item);
            }
        }
    }

    public int GetNextId(string collection)
    {
        lock (_lock)
        {
            return GetNextIdInternal(collection);
        }
    }

    private int GetNextIdInternal(string collection)
    {
        if (!_nextIds.TryGetValue(collection, out var nextId))
        {
            nextId = 1;
            _nextIds[collection] = nextId;
        }
        _nextIds[collection]++;
        return nextId;
    }

    private void EnsureCollection(string collection)
    {
        if (!_data.ContainsKey(collection))
        {
            _data[collection] = new List<Dictionary<string, object?>>();
        }
    }
}
