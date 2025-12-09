using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Caching;

namespace SmartFactory.Infrastructure.Caching;

/// <summary>
/// In-memory implementation of <see cref="ISmartFactoryCache"/> using <see cref="IMemoryCache"/>.
/// </summary>
public class MemorySmartFactoryCache : ISmartFactoryCache
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemorySmartFactoryCache> _logger;
    private readonly ConcurrentDictionary<string, byte> _keys = new();

    public MemorySmartFactoryCache(IMemoryCache cache, ILogger<MemorySmartFactoryCache> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc />
    public T? Get<T>(string key)
    {
        if (_cache.TryGetValue(key, out T? value))
        {
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return value;
        }

        _logger.LogDebug("Cache miss for key: {Key}", key);
        return default;
    }

    /// <inheritdoc />
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Get<T>(key));
    }

    /// <inheritdoc />
    public void Set<T>(string key, T value, CacheEntryOptions? options = null)
    {
        var entryOptions = CreateMemoryCacheEntryOptions(options);

        // Track key removal
        entryOptions.RegisterPostEvictionCallback((evictedKey, _, _, _) =>
        {
            _keys.TryRemove((string)evictedKey, out _);
            _logger.LogDebug("Cache entry evicted: {Key}", evictedKey);
        });

        _cache.Set(key, value, entryOptions);
        _keys.TryAdd(key, 0);

        _logger.LogDebug("Cache set for key: {Key}", key);
    }

    /// <inheritdoc />
    public Task SetAsync<T>(string key, T value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Set(key, value, options);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        CacheEntryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out T? cachedValue) && cachedValue is not null)
        {
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return cachedValue;
        }

        _logger.LogDebug("Cache miss for key: {Key}, creating new entry", key);

        var value = await factory(cancellationToken);
        Set(key, value, options);

        return value;
    }

    /// <inheritdoc />
    public void Remove(string key)
    {
        _cache.Remove(key);
        _keys.TryRemove(key, out _);
        _logger.LogDebug("Cache entry removed: {Key}", key);
    }

    /// <inheritdoc />
    public void RemoveByPattern(string pattern)
    {
        // Convert wildcard pattern to regex
        var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
        var regex = new Regex(regexPattern, RegexOptions.Compiled);

        var keysToRemove = _keys.Keys.Where(k => regex.IsMatch(k)).ToList();

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
        }

        if (keysToRemove.Count > 0)
        {
            _logger.LogDebug("Removed {Count} cache entries matching pattern: {Pattern}", keysToRemove.Count, pattern);
        }
    }

    /// <inheritdoc />
    public bool Exists(string key)
    {
        return _cache.TryGetValue(key, out _);
    }

    private static MemoryCacheEntryOptions CreateMemoryCacheEntryOptions(CacheEntryOptions? options)
    {
        var entryOptions = new MemoryCacheEntryOptions();

        if (options == null)
        {
            // Default: 5 minute absolute expiration
            entryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return entryOptions;
        }

        if (options.AbsoluteExpiration.HasValue)
        {
            entryOptions.AbsoluteExpirationRelativeToNow = options.AbsoluteExpiration.Value;
        }

        if (options.SlidingExpiration.HasValue)
        {
            entryOptions.SlidingExpiration = options.SlidingExpiration.Value;
        }

        entryOptions.Priority = options.Priority switch
        {
            CachePriority.Low => CacheItemPriority.Low,
            CachePriority.Normal => CacheItemPriority.Normal,
            CachePriority.High => CacheItemPriority.High,
            CachePriority.NeverRemove => CacheItemPriority.NeverRemove,
            _ => CacheItemPriority.Normal
        };

        return entryOptions;
    }
}
