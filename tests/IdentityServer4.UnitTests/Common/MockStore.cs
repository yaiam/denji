using System.Collections.Concurrent;
using IdentityServer4.Storage.Stores;

namespace IdentityServer4.UnitTests.Common;

public class MockStore : IStore
{
    private readonly ConcurrentDictionary<string, StoreItem> _items = new();

    public Task<StoreItem> Get(string key, CancellationToken ct)
    {
        _items.TryGetValue(key, out var message);
        return Task.FromResult(message);
    }

    public Task Create(StoreItem item, CancellationToken ct)
    {
        _items[item.Key] = item;
        return Task.CompletedTask;
    }

    public Task Delete(string key, CancellationToken ct)
    {
        _items.Remove(key, out _);
        return Task.CompletedTask;
    }
}