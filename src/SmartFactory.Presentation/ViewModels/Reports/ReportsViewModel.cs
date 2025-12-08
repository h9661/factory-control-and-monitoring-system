using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartFactory.Application.DTOs.Reports;
using SmartFactory.Application.Interfaces;
using SmartFactory.Presentation.Services;
using SmartFactory.Presentation.ViewModels.Base;

namespace SmartFactory.Presentation.ViewModels.Reports;

/// <summary>
/// ViewModel for the reports view.
/// </summary>
public partial class ReportsViewModel : PageViewModelBase
{
    private readonly IReportService _reportService;
    private readonly IFactoryContextService _factoryContext;

    [ObservableProperty]
    private string _selectedReportType = "OEE";

    [ObservableProperty]
    private ObservableCollection<string> _reportTypes = new()
    {
        "OEE", "Production", "Quality", "Maintenance"
    };

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today.AddDays(-30);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today;

    // OEE Report
    [ObservableProperty]
    private OeeReportDto? _oeeReport;

    // Production Report
    [ObservableProperty]
    private ProductionReportDto? _productionReport;

    // Quality Report
    [ObservableProperty]
    private QualityReportDto? _qualityReport;

    // Maintenance Report
    [ObservableProperty]
    private MaintenanceReportDto? _maintenanceReport;

    // Equipment Efficiency
    [ObservableProperty]
    private ObservableCollection<EquipmentEfficiencyDto> _equipmentEfficiency = new();

    [ObservableProperty]
    private bool _isLoading;

    public ReportsViewModel(
        INavigationService navigationService,
        IReportService reportService,
        IFactoryContextService factoryContext)
        : base(navigationService)
    {
        Title = "Reports & Analytics";
        _reportService = reportService;
        _factoryContext = factoryContext;

        _factoryContext.CurrentFactoryChanged += (s, f) => _ = GenerateReportAsync();
    }

    public override async void OnNavigatedTo(object? parameter)
    {
        await GenerateReportAsync();
    }

    [RelayCommand]
    private async Task GenerateReportAsync()
    {
        IsLoading = true;
        await ExecuteAsync(async () =>
        {
            var factoryId = _factoryContext.CurrentFactoryId;

            switch (SelectedReportType)
            {
                case "OEE":
                    OeeReport = await _reportService.GenerateOeeReportAsync(factoryId, StartDate, EndDate);
                    break;
                case "Production":
                    ProductionReport = await _reportService.GenerateProductionReportAsync(factoryId, StartDate, EndDate);
                    break;
                case "Quality":
                    QualityReport = await _reportService.GenerateQualityReportAsync(factoryId, StartDate, EndDate);
                    break;
                case "Maintenance":
                    MaintenanceReport = await _reportService.GenerateMaintenanceReportAsync(factoryId, StartDate, EndDate);
                    break;
            }

            // Always load equipment efficiency
            var efficiency = await _reportService.GetEquipmentEfficiencyAsync(factoryId, StartDate, EndDate);
            EquipmentEfficiency = new ObservableCollection<EquipmentEfficiencyDto>(efficiency);
        }, "Failed to generate report");
        IsLoading = false;
    }

    [RelayCommand]
    private void SetDateRange(string range)
    {
        EndDate = DateTime.Today;
        StartDate = range switch
        {
            "Today" => DateTime.Today,
            "Week" => DateTime.Today.AddDays(-7),
            "Month" => DateTime.Today.AddMonths(-1),
            "Quarter" => DateTime.Today.AddMonths(-3),
            "Year" => DateTime.Today.AddYears(-1),
            _ => DateTime.Today.AddDays(-30)
        };
        _ = GenerateReportAsync();
    }

    partial void OnSelectedReportTypeChanged(string value) => _ = GenerateReportAsync();
    partial void OnStartDateChanged(DateTime value) => _ = GenerateReportAsync();
    partial void OnEndDateChanged(DateTime value) => _ = GenerateReportAsync();
}
