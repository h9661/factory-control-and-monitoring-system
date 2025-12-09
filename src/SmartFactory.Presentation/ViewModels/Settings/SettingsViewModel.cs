using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using SmartFactory.Application.DTOs.Factory;
using SmartFactory.Application.Interfaces;
using SmartFactory.Application.Services.DataSource;
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
        "Factories", "Data Source", "Production Lines", "Application", "About"
    };

    // Data Source Settings
    [ObservableProperty]
    private string _dataSourceMode = "Simulation";

    [ObservableProperty]
    private ObservableCollection<string> _dataSourceModes = new()
    {
        "Simulation", "OPC-UA", "Hybrid"
    };

    [ObservableProperty]
    private string _opcUaServerUrl = "opc.tcp://localhost:4840";

    [ObservableProperty]
    private bool _opcUaSecurityEnabled = false;

    [ObservableProperty]
    private string _opcUaUsername = string.Empty;

    [ObservableProperty]
    private string _opcUaPassword = string.Empty;

    [ObservableProperty]
    private bool _isOpcUaConnected;

    [ObservableProperty]
    private string _opcUaConnectionStatus = "Disconnected";

    [ObservableProperty]
    private bool _isTestingConnection;

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

    // Production Lines
    [ObservableProperty]
    private ObservableCollection<ProductionLineDisplayItem> _productionLines = new();

    [ObservableProperty]
    private ProductionLineDisplayItem? _selectedProductionLine;

    [ObservableProperty]
    private FactoryDisplayItem? _selectedProductionLineFactory;

    // Production Line Dialog
    [ObservableProperty]
    private bool _isProductionLineDialogOpen;

    [ObservableProperty]
    private bool _isProductionLineEditMode;

    [ObservableProperty]
    private string _productionLineCode = string.Empty;

    [ObservableProperty]
    private string _productionLineName = string.Empty;

    [ObservableProperty]
    private int _productionLineSequence = 1;

    [ObservableProperty]
    private string _productionLineDescription = string.Empty;

    [ObservableProperty]
    private int _productionLineDesignedCapacity = 100;

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

            // Select first factory for production lines if available
            if (SelectedProductionLineFactory == null && Factories.Any())
            {
                SelectedProductionLineFactory = Factories.First();
            }
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

    [RelayCommand]
    private async Task TestOpcUaConnectionAsync()
    {
        if (IsTestingConnection) return;

        try
        {
            IsTestingConnection = true;
            OpcUaConnectionStatus = "Testing...";

            // Simulate connection test - in production this would use the actual OPC-UA client
            await Task.Delay(2000); // Simulate network delay

            // For demonstration, we'll simulate success/failure based on URL format
            if (!string.IsNullOrEmpty(OpcUaServerUrl) && OpcUaServerUrl.StartsWith("opc.tcp://"))
            {
                IsOpcUaConnected = true;
                OpcUaConnectionStatus = "Connected";
            }
            else
            {
                IsOpcUaConnected = false;
                OpcUaConnectionStatus = "Failed: Invalid URL format";
            }
        }
        catch (Exception ex)
        {
            IsOpcUaConnected = false;
            OpcUaConnectionStatus = $"Failed: {ex.Message}";
        }
        finally
        {
            IsTestingConnection = false;
        }
    }

    [RelayCommand]
    private void SaveDataSourceSettings()
    {
        // Save data source settings to configuration
        // This would typically persist to appsettings.json or user settings
        // For now, we just show that the settings have been saved
    }

    partial void OnDataSourceModeChanged(string value)
    {
        // React to data source mode changes
        // Could trigger service reinitialization
    }

    // Production Lines Commands
    partial void OnSelectedProductionLineFactoryChanged(FactoryDisplayItem? value)
    {
        if (value != null)
        {
            _ = LoadProductionLinesAsync();
        }
        else
        {
            ProductionLines.Clear();
        }
    }

    [RelayCommand]
    private async Task LoadProductionLinesAsync()
    {
        if (SelectedProductionLineFactory == null) return;

        await ExecuteAsync(async () =>
        {
            var lines = await _factoryService.GetProductionLinesAsync(SelectedProductionLineFactory.Id);
            ProductionLines = new ObservableCollection<ProductionLineDisplayItem>(
                lines.Select(l => new ProductionLineDisplayItem
                {
                    Id = l.Id,
                    Code = l.Code,
                    Name = l.Name,
                    Sequence = l.Sequence,
                    Status = l.Status,
                    DesignedCapacity = l.DesignedCapacity,
                    Description = l.Description,
                    IsActive = l.IsActive,
                    EquipmentCount = l.EquipmentCount
                }));
        });
    }

    [RelayCommand]
    private void ShowAddProductionLineDialog()
    {
        if (SelectedProductionLineFactory == null) return;

        IsProductionLineEditMode = false;
        ProductionLineCode = string.Empty;
        ProductionLineName = string.Empty;
        ProductionLineSequence = ProductionLines.Count + 1;
        ProductionLineDescription = string.Empty;
        ProductionLineDesignedCapacity = 100;
        IsProductionLineDialogOpen = true;
    }

    [RelayCommand]
    private void ShowEditProductionLineDialog(ProductionLineDisplayItem? line)
    {
        if (line == null) return;

        IsProductionLineEditMode = true;
        SelectedProductionLine = line;
        ProductionLineCode = line.Code;
        ProductionLineName = line.Name;
        ProductionLineSequence = line.Sequence;
        ProductionLineDescription = line.Description ?? string.Empty;
        ProductionLineDesignedCapacity = line.DesignedCapacity;
        IsProductionLineDialogOpen = true;
    }

    [RelayCommand]
    private void CloseProductionLineDialog() => IsProductionLineDialogOpen = false;

    [RelayCommand]
    private async Task SaveProductionLineAsync()
    {
        if (SelectedProductionLineFactory == null) return;

        await ExecuteAsync(async () =>
        {
            if (IsProductionLineEditMode && SelectedProductionLine != null)
            {
                var dto = new ProductionLineUpdateDto
                {
                    Name = ProductionLineName,
                    Sequence = ProductionLineSequence,
                    Description = ProductionLineDescription,
                    DesignedCapacity = ProductionLineDesignedCapacity,
                    IsActive = true
                };
                await _factoryService.UpdateProductionLineAsync(SelectedProductionLine.Id, dto);
            }
            else
            {
                var dto = new ProductionLineCreateDto
                {
                    FactoryId = SelectedProductionLineFactory.Id,
                    Code = ProductionLineCode,
                    Name = ProductionLineName,
                    Sequence = ProductionLineSequence,
                    Description = ProductionLineDescription,
                    DesignedCapacity = ProductionLineDesignedCapacity
                };
                await _factoryService.CreateProductionLineAsync(dto);
            }

            IsProductionLineDialogOpen = false;
            await LoadProductionLinesAsync();
        }, "Failed to save production line");
    }

    [RelayCommand]
    private async Task DeleteProductionLineAsync(ProductionLineDisplayItem? line)
    {
        if (line == null) return;

        await ExecuteAsync(async () =>
        {
            await _factoryService.DeleteProductionLineAsync(line.Id);
            await LoadProductionLinesAsync();
        }, "Failed to delete production line");
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

public partial class ProductionLineDisplayItem : ObservableObject
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public string Status { get; set; } = string.Empty;
    public int DesignedCapacity { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int EquipmentCount { get; set; }

    public string StatusDisplay => IsActive ? "Active" : "Inactive";
    public string StatusColor => IsActive ? "#4CAF50" : "#9E9E9E";
    public string CapacityDisplay => $"{DesignedCapacity} units/hr";
}
