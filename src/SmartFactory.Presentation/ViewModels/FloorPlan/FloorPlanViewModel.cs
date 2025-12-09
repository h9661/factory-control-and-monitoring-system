using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartFactory.Application.DTOs.Maintenance;
using SmartFactory.Application.Services.Maintenance;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;
using SmartFactory.Presentation.Models;
using SmartFactory.Presentation.Services;
using SmartFactory.Presentation.ViewModels.Base;
using DomainEquipment = SmartFactory.Domain.Entities.Equipment;

namespace SmartFactory.Presentation.ViewModels.FloorPlan;

/// <summary>
/// ViewModel for the equipment floor plan view.
/// </summary>
public partial class FloorPlanViewModel : ViewModelBase
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IPredictiveMaintenanceService _maintenanceService;
    private readonly IFactoryContextService _factoryContext;
    private readonly DispatcherTimer _refreshTimer;
    private readonly Random _random = new();

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<FloorPlanEquipmentItem> _equipmentItems = new();

    [ObservableProperty]
    private FloorPlanEquipmentItem? _selectedEquipment;

    [ObservableProperty]
    private FloorPlanLayout _layout = new();

    [ObservableProperty]
    private double _zoomLevel = 1.0;

    [ObservableProperty]
    private bool _showGrid = true;

    [ObservableProperty]
    private bool _showConnections = true;

    [ObservableProperty]
    private string _filterStatus = "All";

    [ObservableProperty]
    private ObservableCollection<string> _statusFilters = new()
    {
        "All", "Running", "Idle", "Warning", "Error", "Maintenance", "Stopped"
    };

    // Summary statistics
    [ObservableProperty]
    private int _totalEquipment;

    [ObservableProperty]
    private int _runningCount;

    [ObservableProperty]
    private int _warningCount;

    [ObservableProperty]
    private int _errorCount;

    [ObservableProperty]
    private int _maintenanceCount;

    // Selected equipment details
    [ObservableProperty]
    private double _selectedTemperature;

    [ObservableProperty]
    private double _selectedVibration;

    [ObservableProperty]
    private double _selectedPressure;

    [ObservableProperty]
    private double _selectedHealthScore;

    [ObservableProperty]
    private string _selectedMaintenanceStatus = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    #endregion

    public FloorPlanViewModel(
        IEquipmentRepository equipmentRepository,
        IPredictiveMaintenanceService maintenanceService,
        IFactoryContextService factoryContext)
    {
        _equipmentRepository = equipmentRepository;
        _maintenanceService = maintenanceService;
        _factoryContext = factoryContext;

        Title = "Equipment Floor Plan";
        Description = "Real-time equipment status visualization";

        // Setup refresh timer for real-time updates
        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        _refreshTimer.Tick += async (s, e) => await RefreshSensorDataAsync();

        // Subscribe to factory changes
        _factoryContext.CurrentFactoryChanged += async (s, e) => await LoadEquipmentAsync();
    }

    public async Task LoadAsync()
    {
        await LoadEquipmentAsync();
        _refreshTimer.Start();
    }

    public Task UnloadAsync()
    {
        _refreshTimer.Stop();
        return Task.CompletedTask;
    }

    private async Task LoadEquipmentAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ClearError();

            var factoryId = _factoryContext.CurrentFactoryId;
            if (!factoryId.HasValue) return;

            var equipment = await _equipmentRepository.GetByFactoryAsync(factoryId.Value);
            var healthScores = await _maintenanceService.GetFactoryHealthScoresAsync(factoryId.Value);

            GenerateFloorPlanLayout(equipment.ToList(), healthScores);
            UpdateStatistics();
        }
        catch (Exception ex)
        {
            SetError($"Failed to load equipment: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void GenerateFloorPlanLayout(List<DomainEquipment> equipment, List<HealthScoreDto> healthScores)
    {
        EquipmentItems.Clear();
        Layout = new FloorPlanLayout
        {
            LayoutWidth = 1200,
            LayoutHeight = 800
        };

        // Create zones
        Layout.Zones = new List<FactoryZone>
        {
            new() { Name = "Assembly Line A", Bounds = new System.Windows.Rect(50, 50, 500, 300), Color = "#1E3A5F" },
            new() { Name = "Assembly Line B", Bounds = new System.Windows.Rect(600, 50, 500, 300), Color = "#1E5F3A" },
            new() { Name = "Quality Control", Bounds = new System.Windows.Rect(50, 400, 350, 350), Color = "#5F3A1E" },
            new() { Name = "Packaging", Bounds = new System.Windows.Rect(450, 400, 650, 350), Color = "#3A1E5F" }
        };

        // Position equipment in a grid-like pattern within zones
        var itemsPerRow = 4;
        var horizontalSpacing = 120;
        var verticalSpacing = 120;
        var startX = 100;
        var startY = 100;

        for (int i = 0; i < equipment.Count; i++)
        {
            var eq = equipment[i];
            var healthScore = healthScores.FirstOrDefault(h => h.EquipmentId == eq.Id);

            var row = i / itemsPerRow;
            var col = i % itemsPerRow;

            // Alternate between zones
            var zoneOffset = (row / 2) * 350;
            var rowInZone = row % 2;

            var x = startX + (col * horizontalSpacing) + ((row % 4 >= 2) ? 550 : 0);
            var y = startY + (rowInZone * verticalSpacing) + zoneOffset;

            var item = new FloorPlanEquipmentItem
            {
                EquipmentId = eq.Id,
                Name = eq.Name,
                Code = eq.Code,
                EquipmentType = eq.Type,
                Status = eq.Status,
                X = x,
                Y = y,
                Temperature = GenerateSimulatedSensorValue(eq.Status, 45, 85),
                Vibration = GenerateSimulatedSensorValue(eq.Status, 2, 12),
                Pressure = GenerateSimulatedSensorValue(eq.Status, 90, 130),
                HealthScore = healthScore?.OverallScore ?? 75
            };

            EquipmentItems.Add(item);

            Layout.EquipmentPositions[eq.Id] = new EquipmentPosition
            {
                EquipmentId = eq.Id,
                X = x,
                Y = y
            };
        }

        // Create connections between adjacent equipment
        for (int i = 0; i < EquipmentItems.Count - 1; i++)
        {
            var currentRow = i / itemsPerRow;
            var nextRow = (i + 1) / itemsPerRow;

            // Only connect equipment in the same row
            if (currentRow == nextRow)
            {
                Layout.ProductionLineConnections.Add(new ProductionLineConnection
                {
                    SourceEquipmentId = EquipmentItems[i].EquipmentId,
                    TargetEquipmentId = EquipmentItems[i + 1].EquipmentId,
                    Type = ConnectionType.Flow,
                    IsActive = EquipmentItems[i].Status == EquipmentStatus.Running
                });
            }
        }
    }

    private double GenerateSimulatedSensorValue(EquipmentStatus status, double min, double max)
    {
        var multiplier = status switch
        {
            EquipmentStatus.Running => 0.7,
            EquipmentStatus.Idle => 0.3,
            EquipmentStatus.Warning => 0.85,
            EquipmentStatus.Error => 0.95,
            EquipmentStatus.Maintenance => 0.5,
            _ => 0.4
        };

        var range = max - min;
        return min + (range * multiplier) + (_random.NextDouble() - 0.5) * range * 0.2;
    }

    private async Task RefreshSensorDataAsync()
    {
        foreach (var item in EquipmentItems)
        {
            // Simulate sensor data updates with slight variations
            item.Temperature = GenerateSimulatedSensorValue(item.Status, 45, 85);
            item.Vibration = GenerateSimulatedSensorValue(item.Status, 2, 12);
            item.Pressure = GenerateSimulatedSensorValue(item.Status, 90, 130);

            // Occasionally change status for demo purposes
            if (_random.NextDouble() < 0.02) // 2% chance per update
            {
                item.Status = GetRandomStatus();
            }
        }

        UpdateStatistics();

        // Update selected equipment details
        if (SelectedEquipment != null)
        {
            UpdateSelectedEquipmentDetails();
        }

        await Task.CompletedTask;
    }

    private EquipmentStatus GetRandomStatus()
    {
        var rand = _random.NextDouble();
        return rand switch
        {
            < 0.6 => EquipmentStatus.Running,
            < 0.75 => EquipmentStatus.Idle,
            < 0.85 => EquipmentStatus.Warning,
            < 0.92 => EquipmentStatus.Error,
            < 0.97 => EquipmentStatus.Maintenance,
            _ => EquipmentStatus.Offline
        };
    }

    private void UpdateStatistics()
    {
        var visibleItems = GetFilteredItems().ToList();

        TotalEquipment = EquipmentItems.Count;
        RunningCount = EquipmentItems.Count(e => e.Status == EquipmentStatus.Running);
        WarningCount = EquipmentItems.Count(e => e.Status == EquipmentStatus.Warning);
        ErrorCount = EquipmentItems.Count(e => e.Status == EquipmentStatus.Error);
        MaintenanceCount = EquipmentItems.Count(e => e.Status == EquipmentStatus.Maintenance);
    }

    private IEnumerable<FloorPlanEquipmentItem> GetFilteredItems()
    {
        if (FilterStatus == "All")
            return EquipmentItems;

        return Enum.TryParse<EquipmentStatus>(FilterStatus, out var status)
            ? EquipmentItems.Where(e => e.Status == status)
            : EquipmentItems;
    }

    private void UpdateSelectedEquipmentDetails()
    {
        if (SelectedEquipment == null) return;

        SelectedTemperature = SelectedEquipment.Temperature;
        SelectedVibration = SelectedEquipment.Vibration;
        SelectedPressure = SelectedEquipment.Pressure;
        SelectedHealthScore = SelectedEquipment.HealthScore;
        SelectedMaintenanceStatus = SelectedEquipment.Status.ToString();
    }

    partial void OnSelectedEquipmentChanged(FloorPlanEquipmentItem? value)
    {
        if (value != null)
        {
            UpdateSelectedEquipmentDetails();
        }
    }

    partial void OnFilterStatusChanged(string value)
    {
        // Trigger UI update for filtered items
        OnPropertyChanged(nameof(EquipmentItems));
    }

    [RelayCommand]
    private void SelectEquipment(Guid equipmentId)
    {
        var item = EquipmentItems.FirstOrDefault(e => e.EquipmentId == equipmentId);
        if (item != null)
        {
            // Deselect previous
            if (SelectedEquipment != null)
            {
                SelectedEquipment.IsSelected = false;
            }

            item.IsSelected = true;
            SelectedEquipment = item;
        }
    }

    [RelayCommand]
    private void ZoomIn()
    {
        ZoomLevel = Math.Min(ZoomLevel * 1.25, 4.0);
    }

    [RelayCommand]
    private void ZoomOut()
    {
        ZoomLevel = Math.Max(ZoomLevel / 1.25, 0.25);
    }

    [RelayCommand]
    private void ResetZoom()
    {
        ZoomLevel = 1.0;
    }

    [RelayCommand]
    private void ToggleGrid()
    {
        ShowGrid = !ShowGrid;
    }

    [RelayCommand]
    private void ToggleConnections()
    {
        ShowConnections = !ShowConnections;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadEquipmentAsync();
    }
}

/// <summary>
/// Represents an equipment item on the floor plan.
/// </summary>
public partial class FloorPlanEquipmentItem : ObservableObject
{
    [ObservableProperty]
    private Guid _equipmentId;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _code = string.Empty;

    [ObservableProperty]
    private EquipmentType _equipmentType;

    [ObservableProperty]
    private EquipmentStatus _status;

    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y;

    [ObservableProperty]
    private double _temperature;

    [ObservableProperty]
    private double _vibration;

    [ObservableProperty]
    private double _pressure;

    [ObservableProperty]
    private double _healthScore;

    [ObservableProperty]
    private bool _isSelected;
}
