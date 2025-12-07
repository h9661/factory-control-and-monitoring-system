using System.Windows;
using SmartFactory.Presentation.ViewModels.Base;

namespace SmartFactory.Presentation.Services;

/// <summary>
/// Navigation service implementation for WPF views.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<NavigationEntry> _navigationStack = new();

    public object? CurrentView { get; private set; }
    public bool CanGoBack => _navigationStack.Count > 1;

    public event EventHandler<NavigatedEventArgs>? Navigated;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateTo(Type viewType, object? parameter = null)
    {
        // Get view instance
        var view = _serviceProvider.GetService(viewType);
        if (view == null)
        {
            throw new InvalidOperationException($"View type {viewType.Name} is not registered.");
        }

        // Get ViewModel (convention: ViewName -> ViewNameViewModel)
        var viewModelTypeName = viewType.FullName?.Replace("Views", "ViewModels").Replace("View", "ViewModel");
        if (viewModelTypeName != null)
        {
            var viewModelType = Type.GetType(viewModelTypeName);
            if (viewModelType != null)
            {
                var viewModel = _serviceProvider.GetService(viewModelType);

                if (view is FrameworkElement frameworkElement && viewModel != null)
                {
                    frameworkElement.DataContext = viewModel;
                }

                // Notify previous ViewModel of navigation
                if (CurrentView is FrameworkElement prevElement &&
                    prevElement.DataContext is INavigationAware prevAware)
                {
                    prevAware.OnNavigatedFrom();
                }

                // Notify new ViewModel of navigation
                if (viewModel is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedTo(parameter);
                }
            }
        }

        _navigationStack.Push(new NavigationEntry(viewType, parameter));
        CurrentView = view;

        Navigated?.Invoke(this, new NavigatedEventArgs(viewType, parameter));
    }

    public void NavigateTo<TView>(object? parameter = null) where TView : class
    {
        NavigateTo(typeof(TView), parameter);
    }

    public void GoBack()
    {
        if (!CanGoBack) return;

        _navigationStack.Pop(); // Remove current
        var previous = _navigationStack.Peek();
        NavigateTo(previous.ViewType, previous.Parameter);
    }
}

/// <summary>
/// Record for tracking navigation history.
/// </summary>
internal record NavigationEntry(Type ViewType, object? Parameter);
