using SmartFactory.Domain.Entities;

namespace SmartFactory.Presentation.Services;

/// <summary>
/// Service for managing the currently selected factory context.
/// </summary>
public interface IFactoryContextService
{
    Factory? CurrentFactory { get; }
    Guid? CurrentFactoryId { get; }

    event EventHandler<Factory?>? CurrentFactoryChanged;

    void SetCurrentFactory(Factory factory);
    void ClearCurrentFactory();
}

/// <summary>
/// Implementation of factory context service.
/// </summary>
public class FactoryContextService : IFactoryContextService
{
    private Factory? _currentFactory;

    public Factory? CurrentFactory => _currentFactory;
    public Guid? CurrentFactoryId => _currentFactory?.Id;

    public event EventHandler<Factory?>? CurrentFactoryChanged;

    public void SetCurrentFactory(Factory factory)
    {
        _currentFactory = factory;
        CurrentFactoryChanged?.Invoke(this, factory);
    }

    public void ClearCurrentFactory()
    {
        _currentFactory = null;
        CurrentFactoryChanged?.Invoke(this, null);
    }
}
