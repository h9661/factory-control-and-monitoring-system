using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace SmartFactory.Application.Events;

/// <summary>
/// Thread-safe implementation of the event aggregator pattern.
/// </summary>
public class EventAggregator : IEventAggregator
{
    private readonly ConcurrentDictionary<Type, List<SubscriptionBase>> _subscriptions = new();
    private readonly ILogger<EventAggregator> _logger;
    private readonly object _lock = new();

    public EventAggregator(ILogger<EventAggregator> logger)
    {
        _logger = logger;
    }

    public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
    {
        var subscription = new Subscription<TEvent>(handler, this);
        AddSubscription(typeof(TEvent), subscription);
        return subscription;
    }

    public IDisposable Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class
    {
        var subscription = new AsyncSubscription<TEvent>(handler, this);
        AddSubscription(typeof(TEvent), subscription);
        return subscription;
    }

    public void Publish<TEvent>(TEvent eventMessage) where TEvent : class
    {
        if (eventMessage == null) return;

        var eventType = typeof(TEvent);
        _logger.LogDebug("Publishing event {EventType}", eventType.Name);

        if (_subscriptions.TryGetValue(eventType, out var subscriptions))
        {
            List<SubscriptionBase> subscriptionsCopy;
            lock (_lock)
            {
                subscriptionsCopy = subscriptions.ToList();
            }

            foreach (var subscription in subscriptionsCopy)
            {
                try
                {
                    subscription.Invoke(eventMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error invoking handler for event {EventType}", eventType.Name);
                }
            }
        }
    }

    public async Task PublishAsync<TEvent>(TEvent eventMessage, CancellationToken cancellationToken = default) where TEvent : class
    {
        if (eventMessage == null) return;

        var eventType = typeof(TEvent);
        _logger.LogDebug("Publishing async event {EventType}", eventType.Name);

        if (_subscriptions.TryGetValue(eventType, out var subscriptions))
        {
            List<SubscriptionBase> subscriptionsCopy;
            lock (_lock)
            {
                subscriptionsCopy = subscriptions.ToList();
            }

            var tasks = subscriptionsCopy.Select(async subscription =>
            {
                try
                {
                    await subscription.InvokeAsync(eventMessage, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error invoking async handler for event {EventType}", eventType.Name);
                }
            });

            await Task.WhenAll(tasks);
        }
    }

    private void AddSubscription(Type eventType, SubscriptionBase subscription)
    {
        _subscriptions.AddOrUpdate(
            eventType,
            _ => new List<SubscriptionBase> { subscription },
            (_, list) =>
            {
                lock (_lock)
                {
                    list.Add(subscription);
                }
                return list;
            });
    }

    private void RemoveSubscription(Type eventType, SubscriptionBase subscription)
    {
        if (_subscriptions.TryGetValue(eventType, out var subscriptions))
        {
            lock (_lock)
            {
                subscriptions.Remove(subscription);
            }
        }
    }

    private abstract class SubscriptionBase : IDisposable
    {
        protected readonly EventAggregator Aggregator;
        protected readonly Type EventType;
        private bool _disposed;

        protected SubscriptionBase(EventAggregator aggregator, Type eventType)
        {
            Aggregator = aggregator;
            EventType = eventType;
        }

        public abstract void Invoke(object eventMessage);
        public abstract Task InvokeAsync(object eventMessage, CancellationToken cancellationToken);

        public void Dispose()
        {
            if (!_disposed)
            {
                Aggregator.RemoveSubscription(EventType, this);
                _disposed = true;
            }
        }
    }

    private class Subscription<TEvent> : SubscriptionBase where TEvent : class
    {
        private readonly Action<TEvent> _handler;

        public Subscription(Action<TEvent> handler, EventAggregator aggregator)
            : base(aggregator, typeof(TEvent))
        {
            _handler = handler;
        }

        public override void Invoke(object eventMessage)
        {
            if (eventMessage is TEvent typedEvent)
            {
                _handler(typedEvent);
            }
        }

        public override Task InvokeAsync(object eventMessage, CancellationToken cancellationToken)
        {
            Invoke(eventMessage);
            return Task.CompletedTask;
        }
    }

    private class AsyncSubscription<TEvent> : SubscriptionBase where TEvent : class
    {
        private readonly Func<TEvent, Task> _handler;

        public AsyncSubscription(Func<TEvent, Task> handler, EventAggregator aggregator)
            : base(aggregator, typeof(TEvent))
        {
            _handler = handler;
        }

        public override void Invoke(object eventMessage)
        {
            if (eventMessage is TEvent typedEvent)
            {
                _handler(typedEvent).GetAwaiter().GetResult();
            }
        }

        public override async Task InvokeAsync(object eventMessage, CancellationToken cancellationToken)
        {
            if (eventMessage is TEvent typedEvent)
            {
                await _handler(typedEvent);
            }
        }
    }
}
