using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartFactory.Application.DTOs.Common;
using SmartFactory.Application.DTOs.Maintenance;
using SmartFactory.Application.Interfaces;
using SmartFactory.Domain.Enums;
using SmartFactory.Presentation.Services;
using SmartFactory.Presentation.ViewModels.Base;

namespace SmartFactory.Presentation.ViewModels.Maintenance;

/// <summary>
/// ViewModel for the maintenance management view.
/// </summary>
public partial class MaintenanceViewModel : PageViewModelBase
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly IEquipmentService _equipmentService;
    private readonly IFactoryContextService _factoryContext;

    [ObservableProperty]
    private ObservableCollection<MaintenanceDisplayItem> _maintenanceRecords = new();

    [ObservableProperty]
    private MaintenanceDisplayItem? _selectedRecord;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private MaintenanceStatus? _statusFilter;

    [ObservableProperty]
    private MaintenanceType? _typeFilter;

    [ObservableProperty]
    private ObservableCollection<MaintenanceStatus> _statusOptions = new();

    [ObservableProperty]
    private ObservableCollection<MaintenanceType> _typeOptions = new();

    [ObservableProperty]
    private MaintenanceSummaryDto? _summary;

    [ObservableProperty]
    private ObservableCollection<MaintenanceDueAlertDto> _overdueAlerts = new();

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    // Schedule Dialog
    [ObservableProperty]
    private bool _isScheduleDialogOpen;

    [ObservableProperty]
    private Guid _newEquipmentId;

    [ObservableProperty]
    private string _newTitle = string.Empty;

    [ObservableProperty]
    private string _newDescription = string.Empty;

    [ObservableProperty]
    private MaintenanceType _newType = MaintenanceType.Preventive;

    [ObservableProperty]
    private DateTime _newScheduledDate = DateTime.Today.AddDays(7);

    [ObservableProperty]
    private string _newTechnicianId = string.Empty;

    [ObservableProperty]
    private string _newTechnicianName = string.Empty;

    [ObservableProperty]
    private decimal? _newEstimatedCost;

    // Complete Dialog
    [ObservableProperty]
    private bool _isCompleteDialogOpen;

    [ObservableProperty]
    private decimal? _actualCost;

    [ObservableProperty]
    private int? _downtimeMinutes;

    [ObservableProperty]
    private string _completionNotes = string.Empty;

    [ObservableProperty]
    private string _partsUsed = string.Empty;

    public MaintenanceViewModel(
        INavigationService navigationService,
        IMaintenanceService maintenanceService,
        IEquipmentService equipmentService,
        IFactoryContextService factoryContext)
        : base(navigationService)
    {
        Title = "Maintenance Management";
        _maintenanceService = maintenanceService;
        _equipmentService = equipmentService;
        _factoryContext = factoryContext;

        StatusOptions = new ObservableCollection<MaintenanceStatus>(Enum.GetValues<MaintenanceStatus>());
        TypeOptions = new ObservableCollection<MaintenanceType>(Enum.GetValues<MaintenanceType>());

        _factoryContext.CurrentFactoryChanged += (s, f) => _ = LoadDataAsync();
    }

    public override async void OnNavigatedTo(object? parameter)
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            await LoadMaintenanceRecordsAsync();
            await LoadSummaryAsync();
            await LoadOverdueAlertsAsync();
        });
    }

    private async Task LoadMaintenanceRecordsAsync()
    {
        var filter = new MaintenanceFilterDto
        {
            FactoryId = _factoryContext.CurrentFactoryId,
            Status = StatusFilter,
            Type = TypeFilter,
            SearchText = SearchText
        };

        var pagination = new PaginationDto { PageNumber = CurrentPage, PageSize = 20 };
        var result = await _maintenanceService.GetMaintenanceRecordsAsync(filter, pagination);

        MaintenanceRecords = new ObservableCollection<MaintenanceDisplayItem>(
            result.Items.Select(MapToDisplayItem));
        TotalPages = result.TotalPages;
    }

    private async Task LoadSummaryAsync()
    {
        Summary = await _maintenanceService.GetMaintenanceSummaryAsync(
            _factoryContext.CurrentFactoryId,
            DateTime.Today.AddDays(-30),
            DateTime.Today);
    }

    private async Task LoadOverdueAlertsAsync()
    {
        var alerts = await _maintenanceService.GetOverdueMaintenanceAsync(_factoryContext.CurrentFactoryId);
        OverdueAlerts = new ObservableCollection<MaintenanceDueAlertDto>(alerts);
    }

    [RelayCommand]
    private void ShowScheduleDialog()
    {
        NewTitle = string.Empty;
        NewDescription = string.Empty;
        NewType = MaintenanceType.Preventive;
        NewScheduledDate = DateTime.Today.AddDays(7);
        NewTechnicianId = string.Empty;
        NewTechnicianName = string.Empty;
        NewEstimatedCost = null;
        IsScheduleDialogOpen = true;
    }

    [RelayCommand]
    private void CloseScheduleDialog() => IsScheduleDialogOpen = false;

    [RelayCommand]
    private async Task ScheduleMaintenanceAsync()
    {
        if (NewEquipmentId == Guid.Empty || string.IsNullOrWhiteSpace(NewTitle)) return;

        await ExecuteAsync(async () =>
        {
            var dto = new MaintenanceCreateDto
            {
                EquipmentId = NewEquipmentId,
                Type = NewType,
                Title = NewTitle,
                Description = NewDescription,
                ScheduledDate = NewScheduledDate,
                TechnicianId = NewTechnicianId,
                TechnicianName = NewTechnicianName,
                EstimatedCost = NewEstimatedCost
            };

            await _maintenanceService.ScheduleMaintenanceAsync(dto);
            IsScheduleDialogOpen = false;
            await LoadDataAsync();
        }, "Failed to schedule maintenance");
    }

    [RelayCommand]
    private async Task StartMaintenanceAsync(MaintenanceDisplayItem? record)
    {
        if (record == null) return;
        await ExecuteAsync(async () =>
        {
            await _maintenanceService.StartMaintenanceAsync(record.Id);
            await LoadDataAsync();
        }, "Failed to start maintenance");
    }

    [RelayCommand]
    private void ShowCompleteDialog(MaintenanceDisplayItem? record)
    {
        if (record == null) return;
        SelectedRecord = record;
        ActualCost = null;
        DowntimeMinutes = null;
        CompletionNotes = string.Empty;
        PartsUsed = string.Empty;
        IsCompleteDialogOpen = true;
    }

    [RelayCommand]
    private void CloseCompleteDialog() => IsCompleteDialogOpen = false;

    [RelayCommand]
    private async Task CompleteMaintenanceAsync()
    {
        if (SelectedRecord == null) return;

        await ExecuteAsync(async () =>
        {
            var dto = new MaintenanceCompleteDto
            {
                ActualCost = ActualCost,
                DowntimeMinutes = DowntimeMinutes,
                Notes = CompletionNotes,
                PartsUsed = PartsUsed
            };

            await _maintenanceService.CompleteMaintenanceAsync(SelectedRecord.Id, dto);
            IsCompleteDialogOpen = false;
            await LoadDataAsync();
        }, "Failed to complete maintenance");
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        StatusFilter = null;
        TypeFilter = null;
        CurrentPage = 1;
        _ = LoadMaintenanceRecordsAsync();
    }

    partial void OnSearchTextChanged(string value) => _ = LoadMaintenanceRecordsAsync();
    partial void OnStatusFilterChanged(MaintenanceStatus? value) => _ = LoadMaintenanceRecordsAsync();
    partial void OnTypeFilterChanged(MaintenanceType? value) => _ = LoadMaintenanceRecordsAsync();

    private static MaintenanceDisplayItem MapToDisplayItem(MaintenanceRecordDto record)
    {
        return new MaintenanceDisplayItem
        {
            Id = record.Id,
            EquipmentId = record.EquipmentId,
            EquipmentCode = record.EquipmentCode,
            EquipmentName = record.EquipmentName,
            Type = record.Type,
            Status = record.Status,
            Title = record.Title,
            ScheduledDate = record.ScheduledDate,
            StartedAt = record.StartedAt,
            CompletedAt = record.CompletedAt,
            TechnicianName = record.TechnicianName,
            EstimatedCost = record.EstimatedCost,
            ActualCost = record.ActualCost,
            DowntimeMinutes = record.DowntimeMinutes
        };
    }
}

public partial class MaintenanceDisplayItem : ObservableObject
{
    public Guid Id { get; set; }
    public Guid EquipmentId { get; set; }
    public string EquipmentCode { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public MaintenanceType Type { get; set; }
    public MaintenanceStatus Status { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? TechnicianName { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public int? DowntimeMinutes { get; set; }

    public string TypeDisplay => Type.ToString();
    public string StatusDisplay => Status.ToString();

    public string StatusColor => Status switch
    {
        MaintenanceStatus.Scheduled => "#2196F3",
        MaintenanceStatus.InProgress => "#FF9800",
        MaintenanceStatus.Completed => "#4CAF50",
        MaintenanceStatus.Overdue => "#F44336",
        MaintenanceStatus.Cancelled => "#9E9E9E",
        _ => "#757575"
    };

    public bool CanStart => Status == MaintenanceStatus.Scheduled || Status == MaintenanceStatus.Overdue;
    public bool CanComplete => Status == MaintenanceStatus.InProgress;
}
