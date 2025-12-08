namespace SmartFactory.Application.Events;

/// <summary>
/// Interface for the event aggregator pattern enabling pub/sub messaging.
/// </summary>
public interface IEventAggregator
{
    /// <summary>
    /// Subscribe to an event type.
    /// </summary>
    /// <typeparam name="TEvent">The event type to subscribe to.</typeparam>
    /// <param name="handler">The handler to invoke when the event is published.</param>
    /// <returns>A subscription token that can be used to unsubscribe.</returns>
    IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;

    /// <summary>
    /// Subscribe to an event type with async handler.
    /// </summary>
    /// <typeparam name="TEvent">The event type to subscribe to.</typeparam>
    /// <param name="handler">The async handler to invoke when the event is published.</param>
    /// <returns>A subscription token that can be used to unsubscribe.</returns>
    IDisposable Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class;

    /// <summary>
    /// Publish an event to all subscribers.
    /// </summary>
    /// <typeparam name="TEvent">The event type to publish.</typeparam>
    /// <param name="eventMessage">The event message to publish.</param>
    void Publish<TEvent>(TEvent eventMessage) where TEvent : class;

    /// <summary>
    /// Publish an event to all subscribers asynchronously.
    /// </summary>
    /// <typeparam name="TEvent">The event type to publish.</typeparam>
    /// <param name="eventMessage">The event message to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishAsync<TEvent>(TEvent eventMessage, CancellationToken cancellationToken = default) where TEvent : class;
}

/// <summary>
/// Base class for all domain events.
/// </summary>
public abstract record DomainEvent
{
    /// <summary>
    /// Unique identifier for the event.
    /// </summary>
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
