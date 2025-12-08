using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartFactory.Application.DTOs.Factory;
using SmartFactory.Application.Interfaces;
using SmartFactory.Presentation.Services;
using SmartFactory.Presentation.ViewModels.Base;

namespace SmartFactory.Presentation.ViewModels.Settings;

/// <summary>
/// ViewModel for the settings view.
/// </summary>
public partial class SettingsViewModel : PageViewModelBase
{
    private readonly IFactoryService _factoryService;
    private readonly IFactoryContextService _factoryContext;

    [ObservableProperty]
    private ObservableCollection<FactoryDisplayItem> _factories = new();

    [ObservableProperty]
    private FactoryDisplayItem? _selectedFactory;

    [ObservableProperty]
    private string _selectedTab = "Factories";

    [ObservableProperty]
    private ObservableCollection<string> _tabs = new()
    {
        "Factories", "Production Lines", "Application", "About"
    };

    // Application Settings
    [ObservableProperty]
    private int _refreshInterval = 30;

    [ObservableProperty]
    private bool _enableNotifications = true;

    [ObservableProperty]
    private bool _enableSounds = true;

    [ObservableProperty]
    private string _theme = "Dark";

    // Factory Dialog
    [ObservableProperty]
    private bool _isFactoryDialogOpen;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private string _factoryCode = string.Empty;

    [ObservableProperty]
    private string _factoryName = string.Empty;

    [ObservableProperty]
    private string _factoryLocation = string.Empty;

    [ObservableProperty]
    private string _factoryDescription = string.Empty;

    [ObservableProperty]
    private string _factoryTimeZone = "UTC";

    [ObservableProperty]
    private string _factoryContactEmail = string.Empty;

    [ObservableProperty]
    private string _factoryContactPhone = string.Empty;

    // About
    [ObservableProperty]
    private string _appVersion = "1.0.0";

    [ObservableProperty]
    private string _appName = "Smart Factory Control System";

    public SettingsViewModel(
        INavigationService navigationService,
        IFactoryService factoryService,
        IFactoryContextService factoryContext)
        : base(navigationService)
    {
        Title = "Settings";
        _factoryService = factoryService;
        _factoryContext = factoryContext;
    }

    public override async void OnNavigatedTo(object? parameter)
    {
        await LoadFactoriesAsync();
    }

    [RelayCommand]
    private async Task LoadFactoriesAsync()
    {
        await ExecuteAsync(async () =>
        {
            var factories = await _factoryService.GetAllFactoriesAsync();
            Factories = new ObservableCollection<FactoryDisplayItem>(
                factories.Select(f => new FactoryDisplayItem
                {
                    Id = f.Id,
                    Code = f.Code,
                    Name = f.Name,
                    Location = f.Location,
                    IsActive = f.IsActive,
                    TimeZone = f.TimeZone
                }));
        });
    }

    [RelayCommand]
    private void ShowAddFactoryDialog()
    {
        IsEditMode = false;
        FactoryCode = string.Empty;
        FactoryName = string.Empty;
        FactoryLocation = string.Empty;
        FactoryDescription = string.Empty;
        FactoryTimeZone = "UTC";
        FactoryContactEmail = string.Empty;
        FactoryContactPhone = string.Empty;
        IsFactoryDialogOpen = true;
    }

    [RelayCommand]
    private async Task ShowEditFactoryDialogAsync(FactoryDisplayItem? factory)
    {
        if (factory == null) return;

        await ExecuteAsync(async () =>
        {
            var detail = await _factoryService.GetFactoryByIdAsync(factory.Id);
            if (detail == null) return;

            IsEditMode = true;
            SelectedFactory = factory;
            FactoryCode = detail.Code;
            FactoryName = detail.Name;
            FactoryLocation = detail.Location ?? string.Empty;
            FactoryDescription = detail.Description ?? string.Empty;
            FactoryTimeZone = detail.TimeZone;
            FactoryContactEmail = detail.ContactEmail ?? string.Empty;
            FactoryContactPhone = detail.ContactPhone ?? string.Empty;
            IsFactoryDialogOpen = true;
        });
    }

    [RelayCommand]
    private void CloseFactoryDialog() => IsFactoryDialogOpen = false;

    [RelayCommand]
    private async Task SaveFactoryAsync()
    {
        await ExecuteAsync(async () =>
        {
            if (IsEditMode && SelectedFactory != null)
            {
                var dto = new FactoryUpdateDto
                {
                    Name = FactoryName,
                    Location = FactoryLocation,
                    Description = FactoryDescription,
                    TimeZone = FactoryTimeZone,
                    ContactEmail = FactoryContactEmail,
                    ContactPhone = FactoryContactPhone,
                    IsActive = true
                };
                await _factoryService.UpdateFactoryAsync(SelectedFactory.Id, dto);
            }
            else
            {
                var dto = new FactoryCreateDto
                {
                    Code = FactoryCode,
                    Name = FactoryName,
                    Location = FactoryLocation,
                    Description = FactoryDescription,
                    TimeZone = FactoryTimeZone,
                    ContactEmail = FactoryContactEmail,
                    ContactPhone = FactoryContactPhone
                };
                await _factoryService.CreateFactoryAsync(dto);
            }

            IsFactoryDialogOpen = false;
            await LoadFactoriesAsync();
        }, "Failed to save factory");
    }

    [RelayCommand]
    private async Task DeleteFactoryAsync(FactoryDisplayItem? factory)
    {
        if (factory == null) return;

        await ExecuteAsync(async () =>
        {
            await _factoryService.DeleteFactoryAsync(factory.Id);
            await LoadFactoriesAsync();
        }, "Failed to delete factory");
    }

    [RelayCommand]
    private async Task SetCurrentFactoryAsync(FactoryDisplayItem? factory)
    {
        if (factory == null) return;

        await ExecuteAsync(async () =>
        {
            var detail = await _factoryService.GetFactoryByIdAsync(factory.Id);
            if (detail != null)
            {
                // Note: This would need to map to a Factory entity
                // For now, we're just showing the pattern
            }
        });
    }

    [RelayCommand]
    private void SaveAppSettings()
    {
        // Save to app settings/config
        // This would typically persist to appsettings.json or user settings
    }

    [RelayCommand]
    private void SelectTab(string? tab)
    {
        if (!string.IsNullOrEmpty(tab))
        {
            SelectedTab = tab;
        }
    }
}

public partial class FactoryDisplayItem : ObservableObject
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public bool IsActive { get; set; }
    public string TimeZone { get; set; } = "UTC";

    public string StatusDisplay => IsActive ? "Active" : "Inactive";
    public string StatusColor => IsActive ? "#4CAF50" : "#9E9E9E";
}
