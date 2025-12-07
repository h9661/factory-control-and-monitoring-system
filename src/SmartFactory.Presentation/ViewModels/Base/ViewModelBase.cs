using CommunityToolkit.Mvvm.ComponentModel;

namespace SmartFactory.Presentation.ViewModels.Base;

/// <summary>
/// Base class for all ViewModels providing common functionality.
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private string? _errorMessage;

    public bool IsNotBusy => !IsBusy;

    protected virtual Task InitializeAsync() => Task.CompletedTask;

    protected void SetError(string message)
    {
        ErrorMessage = message;
    }

    protected void ClearError()
    {
        ErrorMessage = null;
    }

    protected async Task ExecuteAsync(Func<Task> action, string? errorMessage = null)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ClearError();
            await action();
        }
        catch (Exception ex)
        {
            SetError(errorMessage ?? ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}

/// <summary>
/// Base class for page ViewModels with navigation support.
/// </summary>
public abstract partial class PageViewModelBase : ViewModelBase, INavigationAware
{
    protected readonly INavigationService NavigationService;

    protected PageViewModelBase(INavigationService navigationService)
    {
        NavigationService = navigationService;
    }

    public virtual void OnNavigatedTo(object? parameter) { }
    public virtual void OnNavigatedFrom() { }
}

/// <summary>
/// Interface for ViewModels that need to be notified of navigation events.
/// </summary>
public interface INavigationAware
{
    void OnNavigatedTo(object? parameter);
    void OnNavigatedFrom();
}
