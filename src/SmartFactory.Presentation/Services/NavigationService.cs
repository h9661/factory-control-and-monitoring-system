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

        // Notify previous ViewModel of navigation (do this before changing CurrentView)
        if (CurrentView is FrameworkElement prevElement &&
            prevElement.DataContext is INavigationAware prevAware)
        {
            prevAware.OnNavigatedFrom();
        }

        // Get ViewModel by convention: ViewName -> ViewNameViewModel
        // Replace namespace .Views. with .ViewModels., then append "Model" to class name
        var viewModelTypeName = viewType.FullName?.Replace(".Views.", ".ViewModels.");
        if (viewModelTypeName != null && viewModelTypeName.EndsWith("View"))
        {
            viewModelTypeName += "Model"; // DashboardView -> DashboardViewModel
        }

        object? viewModel = null;

        if (viewModelTypeName != null)
        {
            var viewModelType = viewType.Assembly.GetType(viewModelTypeName);

            if (viewModelType != null)
            {
                viewModel = _serviceProvider.GetService(viewModelType);
            }

            // Log when ViewModel resolution fails for debugging
            if (viewModel == null)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[NavigationService] Warning: Could not resolve ViewModel for {viewType.Name}. " +
                    $"Tried: {viewModelTypeName}");
            }
        }

        // Always set DataContext on FrameworkElement to prevent parent inheritance
        if (view is FrameworkElement frameworkElement)
        {
            // Set DataContext even if null to prevent inheriting ShellViewModel
            frameworkElement.DataContext = viewModel;

            if (viewModel == null)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[NavigationService] Warning: DataContext set to null for {viewType.Name}");
            }
        }

        // Notify new ViewModel of navigation
        if (viewModel is INavigationAware navigationAware)
        {
            navigationAware.OnNavigatedTo(parameter);
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
