namespace SmartFactory.Presentation.Services;

/// <summary>
/// Service interface for navigation between views.
/// </summary>
public interface INavigationService
{
    object? CurrentView { get; }
    bool CanGoBack { get; }

    void NavigateTo(Type viewType, object? parameter = null);
    void NavigateTo<TView>(object? parameter = null) where TView : class;
    void GoBack();

    event EventHandler<NavigatedEventArgs>? Navigated;
}

/// <summary>
/// Event arguments for navigation events.
/// </summary>
public class NavigatedEventArgs : EventArgs
{
    public Type ViewType { get; }
    public object? Parameter { get; }

    public NavigatedEventArgs(Type viewType, object? parameter)
    {
        ViewType = viewType;
        Parameter = parameter;
    }
}
