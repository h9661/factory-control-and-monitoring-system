namespace SmartFactory.Application.Caching;

/// <summary>
/// Abstraction for caching operations in the Smart Factory system.
/// </summary>
public interface ISmartFactoryCache
{
    /// <summary>
    /// Gets a cached value by key.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <returns>The cached value, or default if not found.</returns>
    T? Get<T>(string key);

    /// <summary>
    /// Gets a cached value by key asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached value, or default if not found.</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a value in the cache with the specified key and options.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="options">Cache entry options.</param>
    void Set<T>(string key, T value, CacheEntryOptions? options = null);

    /// <summary>
    /// Sets a value in the cache asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="options">Cache entry options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetAsync<T>(string key, T value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or creates a cached value.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">Factory to create the value if not cached.</param>
    /// <param name="options">Cache entry options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached or newly created value.</returns>
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        CacheEntryOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    void Remove(string key);

    /// <summary>
    /// Removes all values with keys matching the specified pattern.
    /// </summary>
    /// <param name="pattern">The key pattern to match (e.g., "alarm:*").</param>
    void RemoveByPattern(string pattern);

    /// <summary>
    /// Checks if a key exists in the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>True if the key exists; otherwise, false.</returns>
    bool Exists(string key);
}

/// <summary>
/// Options for cache entries.
/// </summary>
public class CacheEntryOptions
{
    /// <summary>
    /// Gets or sets the absolute expiration time.
    /// </summary>
    public TimeSpan? AbsoluteExpiration { get; set; }

    /// <summary>
    /// Gets or sets the sliding expiration time.
    /// </summary>
    public TimeSpan? SlidingExpiration { get; set; }

    /// <summary>
    /// Gets or sets the priority of the cache entry.
    /// </summary>
    public CachePriority Priority { get; set; } = CachePriority.Normal;

    /// <summary>
    /// Creates options with absolute expiration.
    /// </summary>
    public static CacheEntryOptions Absolute(TimeSpan expiration) =>
        new() { AbsoluteExpiration = expiration };

    /// <summary>
    /// Creates options with sliding expiration.
    /// </summary>
    public static CacheEntryOptions Sliding(TimeSpan expiration) =>
        new() { SlidingExpiration = expiration };

    /// <summary>
    /// Creates options with both absolute and sliding expiration.
    /// </summary>
    public static CacheEntryOptions AbsoluteWithSliding(TimeSpan absolute, TimeSpan sliding) =>
        new() { AbsoluteExpiration = absolute, SlidingExpiration = sliding };
}

/// <summary>
/// Cache entry priority levels.
/// </summary>
public enum CachePriority
{
    Low,
    Normal,
    High,
    NeverRemove
}
