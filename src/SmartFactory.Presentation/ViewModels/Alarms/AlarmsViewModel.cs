using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartFactory.Application.DTOs.Alarm;
using SmartFactory.Application.DTOs.Common;
using SmartFactory.Application.Interfaces;
using SmartFactory.Domain.Enums;
using SmartFactory.Presentation.Services;
using SmartFactory.Presentation.ViewModels.Base;

namespace SmartFactory.Presentation.ViewModels.Alarms;

/// <summary>
/// ViewModel for the alarms management view.
/// </summary>
public partial class AlarmsViewModel : PageViewModelBase
{
    private readonly IAlarmService _alarmService;
    private readonly IFactoryContextService _factoryContext;

    [ObservableProperty]
    private ObservableCollection<AlarmDisplayItem> _alarms = new();

    [ObservableProperty]
    private ObservableCollection<AlarmDisplayItem> _selectedAlarms = new();

    [ObservableProperty]
    private AlarmDisplayItem? _selectedAlarm;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private AlarmSeverity? _severityFilter;

    [ObservableProperty]
    private AlarmStatus? _statusFilter;

    [ObservableProperty]
    private bool _activeOnlyFilter = true;

    [ObservableProperty]
    private ObservableCollection<AlarmSeverity> _severityOptions = new();

    [ObservableProperty]
    private ObservableCollection<AlarmStatus> _statusOptions = new();

    [ObservableProperty]
    private AlarmSummaryDto? _alarmSummary;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _pageSize = 20;

    [ObservableProperty]
    private string _acknowledgeUserId = "CurrentUser";

    [ObservableProperty]
    private string _resolveNotes = string.Empty;

    [ObservableProperty]
    private bool _isAcknowledgeDialogOpen;

    [ObservableProperty]
    private bool _isResolveDialogOpen;

    public AlarmsViewModel(
        INavigationService navigationService,
        IAlarmService alarmService,
        IFactoryContextService factoryContext)
        : base(navigationService)
    {
        Title = "Alarm Management";
        _alarmService = alarmService;
        _factoryContext = factoryContext;

        // Initialize filter options
        SeverityOptions = new ObservableCollection<AlarmSeverity>(Enum.GetValues<AlarmSeverity>());
        StatusOptions = new ObservableCollection<AlarmStatus>(Enum.GetValues<AlarmStatus>());

        _factoryContext.CurrentFactoryChanged += (s, f) => _ = LoadAlarmsAsync();
    }

    public override async void OnNavigatedTo(object? parameter)
    {
        await LoadAlarmsAsync();
        await LoadAlarmSummaryAsync();
    }

    [RelayCommand]
    private async Task LoadAlarmsAsync()
    {
        await ExecuteAsync(async () =>
        {
            var filter = new AlarmFilterDto
            {
                FactoryId = _factoryContext.CurrentFactoryId,
                Severity = SeverityFilter,
                Status = StatusFilter,
                ActiveOnly = ActiveOnlyFilter,
                SearchText = SearchText
            };

            var pagination = new PaginationDto
            {
                PageNumber = CurrentPage,
                PageSize = PageSize
            };

            var result = await _alarmService.GetAlarmsAsync(filter, pagination);

            Alarms = new ObservableCollection<AlarmDisplayItem>(
                result.Items.Select(a => MapToDisplayItem(a)));

            TotalPages = result.TotalPages;
        });
    }

    [RelayCommand]
    private async Task LoadAlarmSummaryAsync()
    {
        await ExecuteAsync(async () =>
        {
            AlarmSummary = await _alarmService.GetAlarmSummaryAsync(_factoryContext.CurrentFactoryId);
        });
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAlarmsAsync();
        await LoadAlarmSummaryAsync();
    }

    [RelayCommand]
    private void ShowAcknowledgeDialog()
    {
        if (SelectedAlarm == null && !SelectedAlarms.Any()) return;
        IsAcknowledgeDialogOpen = true;
    }

    [RelayCommand]
    private void CloseAcknowledgeDialog()
    {
        IsAcknowledgeDialogOpen = false;
    }

    [RelayCommand]
    private async Task AcknowledgeAlarmAsync()
    {
        if (string.IsNullOrEmpty(AcknowledgeUserId)) return;

        await ExecuteAsync(async () =>
        {
            if (SelectedAlarms.Count > 0)
            {
                var ids = SelectedAlarms.Select(a => a.Id).ToList();
                await _alarmService.AcknowledgeAlarmsAsync(ids, AcknowledgeUserId);
            }
            else if (SelectedAlarm != null)
            {
                var dto = new AlarmAcknowledgeDto { UserId = AcknowledgeUserId };
                await _alarmService.AcknowledgeAlarmAsync(SelectedAlarm.Id, dto);
            }

            IsAcknowledgeDialogOpen = false;
            await LoadAlarmsAsync();
            await LoadAlarmSummaryAsync();
        }, "Failed to acknowledge alarm(s)");
    }

    [RelayCommand]
    private void ShowResolveDialog()
    {
        if (SelectedAlarm == null) return;
        ResolveNotes = string.Empty;
        IsResolveDialogOpen = true;
    }

    [RelayCommand]
    private void CloseResolveDialog()
    {
        IsResolveDialogOpen = false;
    }

    [RelayCommand]
    private async Task ResolveAlarmAsync()
    {
        if (SelectedAlarm == null || string.IsNullOrEmpty(AcknowledgeUserId)) return;

        await ExecuteAsync(async () =>
        {
            var dto = new AlarmResolveDto
            {
                UserId = AcknowledgeUserId,
                ResolutionNotes = ResolveNotes
            };

            await _alarmService.ResolveAlarmAsync(SelectedAlarm.Id, dto);

            IsResolveDialogOpen = false;
            await LoadAlarmsAsync();
            await LoadAlarmSummaryAsync();
        }, "Failed to resolve alarm");
    }

    [RelayCommand]
    private async Task AcknowledgeAllActiveAsync()
    {
        if (Alarms.Count == 0) return;

        await ExecuteAsync(async () =>
        {
            var activeAlarmIds = Alarms
                .Where(a => a.Status == "Active")
                .Select(a => a.Id)
                .ToList();

            if (activeAlarmIds.Count > 0)
            {
                await _alarmService.AcknowledgeAlarmsAsync(activeAlarmIds, AcknowledgeUserId);
                await LoadAlarmsAsync();
                await LoadAlarmSummaryAsync();
            }
        }, "Failed to acknowledge all alarms");
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        SeverityFilter = null;
        StatusFilter = null;
        ActiveOnlyFilter = true;
        CurrentPage = 1;
        _ = LoadAlarmsAsync();
    }

    [RelayCommand]
    private void FilterBySeverity(AlarmSeverity severity)
    {
        SeverityFilter = severity;
        CurrentPage = 1;
        _ = LoadAlarmsAsync();
    }

    [RelayCommand]
    private async Task GoToPreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadAlarmsAsync();
        }
    }

    [RelayCommand]
    private async Task GoToNextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadAlarmsAsync();
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        CurrentPage = 1;
        _ = LoadAlarmsAsync();
    }

    partial void OnSeverityFilterChanged(AlarmSeverity? value)
    {
        CurrentPage = 1;
        _ = LoadAlarmsAsync();
    }

    partial void OnStatusFilterChanged(AlarmStatus? value)
    {
        CurrentPage = 1;
        _ = LoadAlarmsAsync();
    }

    partial void OnActiveOnlyFilterChanged(bool value)
    {
        CurrentPage = 1;
        _ = LoadAlarmsAsync();
    }

    private static AlarmDisplayItem MapToDisplayItem(AlarmDto alarm)
    {
        return new AlarmDisplayItem
        {
            Id = alarm.Id,
            AlarmCode = alarm.AlarmCode,
            Message = alarm.Message,
            Severity = alarm.Severity.ToString(),
            SeverityValue = alarm.Severity,
            Status = alarm.Status.ToString(),
            StatusValue = alarm.Status,
            EquipmentId = alarm.EquipmentId,
            EquipmentCode = alarm.EquipmentCode,
            EquipmentName = alarm.EquipmentName,
            OccurredAt = alarm.OccurredAt,
            AcknowledgedAt = alarm.AcknowledgedAt,
            AcknowledgedBy = alarm.AcknowledgedBy,
            ResolvedAt = alarm.ResolvedAt,
            ResolvedBy = alarm.ResolvedBy,
            IsActive = alarm.IsActive
        };
    }
}

/// <summary>
/// Display model for alarms in the list.
/// </summary>
public partial class AlarmDisplayItem : ObservableObject
{
    public Guid Id { get; set; }
    public string AlarmCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public AlarmSeverity SeverityValue { get; set; }
    public string Status { get; set; } = string.Empty;
    public AlarmStatus StatusValue { get; set; }
    public Guid EquipmentId { get; set; }
    public string EquipmentCode { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public string? AcknowledgedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
    public bool IsActive { get; set; }

    [ObservableProperty]
    private bool _isSelected;

    public string SeverityColor => SeverityValue switch
    {
        AlarmSeverity.Critical => "#E53935",
        AlarmSeverity.Error => "#FF5722",
        AlarmSeverity.Warning => "#FFC107",
        AlarmSeverity.Information => "#2196F3",
        _ => "#757575"
    };

    public string StatusColor => StatusValue switch
    {
        AlarmStatus.Active => "#E53935",
        AlarmStatus.Acknowledged => "#FFC107",
        AlarmStatus.Resolved => "#4CAF50",
        _ => "#757575"
    };

    public string Duration
    {
        get
        {
            var endTime = ResolvedAt ?? DateTime.UtcNow;
            var duration = endTime - OccurredAt;

            if (duration.TotalDays >= 1)
                return $"{duration.Days}d {duration.Hours}h";
            if (duration.TotalHours >= 1)
                return $"{duration.Hours}h {duration.Minutes}m";
            return $"{duration.Minutes}m {duration.Seconds}s";
        }
    }
}
