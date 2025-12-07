using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartFactory.Domain.Interfaces;
using SmartFactory.Presentation.Services;
using SmartFactory.Presentation.ViewModels.Base;
using SmartFactory.Presentation.Views.Equipment;

namespace SmartFactory.Presentation.ViewModels.Dashboard;

/// <summary>
/// ViewModel for the main dashboard view.
/// </summary>
public partial class DashboardViewModel : PageViewModelBase
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IAlarmRepository _alarmRepository;
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IFactoryContextService _factoryContext;

    [ObservableProperty]
    private EquipmentStatusSummary? _equipmentSummary;

    [ObservableProperty]
    private ProductionSummary? _productionSummary;

    [ObservableProperty]
    private AlarmSummary? _alarmSummary;

    [ObservableProperty]
    private ObservableCollection<AlarmDisplayItem> _recentAlarms = new();

    [ObservableProperty]
    private double _overallEfficiency;

    [ObservableProperty]
    private int _activeEquipmentCount;

    [ObservableProperty]
    private int _totalEquipmentCount;

    public DashboardViewModel(
        INavigationService navigationService,
        IEquipmentRepository equipmentRepository,
        IAlarmRepository alarmRepository,
        IWorkOrderRepository workOrderRepository,
        IFactoryContextService factoryContext)
        : base(navigationService)
    {
        Title = "Dashboard";
        _equipmentRepository = equipmentRepository;
        _alarmRepository = alarmRepository;
        _workOrderRepository = workOrderRepository;
        _factoryContext = factoryContext;

        _factoryContext.CurrentFactoryChanged += (s, f) => _ = LoadDashboardDataAsync();
    }

    public override async void OnNavigatedTo(object? parameter)
    {
        await LoadDashboardDataAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDashboardDataAsync();
    }

    [RelayCommand]
    private void NavigateToEquipment(Guid equipmentId)
    {
        NavigationService.NavigateTo(typeof(EquipmentDetailView), equipmentId);
    }

    private async Task LoadDashboardDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            var factoryId = _factoryContext.CurrentFactoryId;

            // Load equipment summary
            EquipmentSummary = await _equipmentRepository.GetStatusSummaryAsync(factoryId);
            TotalEquipmentCount = EquipmentSummary.TotalCount;
            ActiveEquipmentCount = EquipmentSummary.RunningCount + EquipmentSummary.IdleCount;

            // Load production summary
            ProductionSummary = await _workOrderRepository.GetProductionSummaryAsync(factoryId, DateTime.Today);
            OverallEfficiency = ProductionSummary?.YieldRate ?? 0;

            // Load alarm summary
            AlarmSummary = await _alarmRepository.GetAlarmSummaryAsync(factoryId);

            // Load recent alarms
            var alarms = await _alarmRepository.GetRecentAlarmsAsync(10, factoryId);
            RecentAlarms = new ObservableCollection<AlarmDisplayItem>(
                alarms.Select(a => new AlarmDisplayItem
                {
                    Id = a.Id,
                    AlarmCode = a.AlarmCode,
                    Message = a.Message,
                    Severity = a.Severity.ToString(),
                    EquipmentName = a.Equipment.Name,
                    OccurredAt = a.OccurredAt
                }));
        });
    }
}

/// <summary>
/// Display model for alarms in the dashboard.
/// </summary>
public class AlarmDisplayItem
{
    public Guid Id { get; set; }
    public string AlarmCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
}
