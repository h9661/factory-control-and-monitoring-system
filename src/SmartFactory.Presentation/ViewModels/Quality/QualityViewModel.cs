using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartFactory.Application.DTOs.Common;
using SmartFactory.Application.DTOs.Quality;
using SmartFactory.Application.Interfaces;
using SmartFactory.Domain.Enums;
using SmartFactory.Presentation.Services;
using SmartFactory.Presentation.ViewModels.Base;

namespace SmartFactory.Presentation.ViewModels.Quality;

/// <summary>
/// ViewModel for the quality management view.
/// </summary>
public partial class QualityViewModel : PageViewModelBase
{
    private readonly IQualityService _qualityService;
    private readonly IFactoryContextService _factoryContext;

    [ObservableProperty]
    private ObservableCollection<QualityDisplayItem> _qualityRecords = new();

    [ObservableProperty]
    private QualityDisplayItem? _selectedRecord;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private InspectionType? _inspectionTypeFilter;

    [ObservableProperty]
    private InspectionResult? _resultFilter;

    [ObservableProperty]
    private ObservableCollection<InspectionType> _inspectionTypeOptions = new();

    [ObservableProperty]
    private ObservableCollection<InspectionResult> _resultOptions = new();

    [ObservableProperty]
    private DefectSummaryDto? _defectSummary;

    [ObservableProperty]
    private ObservableCollection<QualityTrendDto> _qualityTrends = new();

    [ObservableProperty]
    private double _todayYieldRate;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    // Inspection Dialog
    [ObservableProperty]
    private bool _isInspectionDialogOpen;

    [ObservableProperty]
    private Guid _newEquipmentId;

    [ObservableProperty]
    private Guid? _newWorkOrderId;

    [ObservableProperty]
    private InspectionType _newInspectionType = InspectionType.Visual;

    [ObservableProperty]
    private InspectionResult _newResult = InspectionResult.Pass;

    [ObservableProperty]
    private DefectType? _newDefectType;

    [ObservableProperty]
    private string _newDefectDescription = string.Empty;

    [ObservableProperty]
    private int? _newDefectCount;

    [ObservableProperty]
    private int? _newSampleSize;

    [ObservableProperty]
    private string _newInspectorId = string.Empty;

    [ObservableProperty]
    private string _newInspectorName = string.Empty;

    [ObservableProperty]
    private string _newNotes = string.Empty;

    public QualityViewModel(
        INavigationService navigationService,
        IQualityService qualityService,
        IFactoryContextService factoryContext)
        : base(navigationService)
    {
        Title = "Quality Control";
        _qualityService = qualityService;
        _factoryContext = factoryContext;

        InspectionTypeOptions = new ObservableCollection<InspectionType>(Enum.GetValues<InspectionType>());
        ResultOptions = new ObservableCollection<InspectionResult>(Enum.GetValues<InspectionResult>());

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
            await LoadQualityRecordsAsync();
            await LoadDefectSummaryAsync();
            await LoadQualityTrendsAsync();
            TodayYieldRate = await _qualityService.CalculateYieldRateAsync(
                _factoryContext.CurrentFactoryId, DateTime.Today);
        });
    }

    private async Task LoadQualityRecordsAsync()
    {
        var filter = new QualityFilterDto
        {
            FactoryId = _factoryContext.CurrentFactoryId,
            InspectionType = InspectionTypeFilter,
            Result = ResultFilter,
            SearchText = SearchText
        };

        var pagination = new PaginationDto { PageNumber = CurrentPage, PageSize = 20 };
        var result = await _qualityService.GetQualityRecordsAsync(filter, pagination);

        QualityRecords = new ObservableCollection<QualityDisplayItem>(
            result.Items.Select(MapToDisplayItem));
        TotalPages = result.TotalPages;
    }

    private async Task LoadDefectSummaryAsync()
    {
        DefectSummary = await _qualityService.GetDefectSummaryAsync(
            _factoryContext.CurrentFactoryId,
            DateTime.Today.AddDays(-30),
            DateTime.Today);
    }

    private async Task LoadQualityTrendsAsync()
    {
        var trends = await _qualityService.GetQualityTrendsAsync(
            _factoryContext.CurrentFactoryId,
            DateTime.Today.AddDays(-7),
            DateTime.Today);
        QualityTrends = new ObservableCollection<QualityTrendDto>(trends);
    }

    [RelayCommand]
    private void ShowInspectionDialog()
    {
        NewInspectionType = InspectionType.Visual;
        NewResult = InspectionResult.Pass;
        NewDefectType = null;
        NewDefectDescription = string.Empty;
        NewDefectCount = null;
        NewSampleSize = null;
        NewInspectorId = string.Empty;
        NewInspectorName = string.Empty;
        NewNotes = string.Empty;
        IsInspectionDialogOpen = true;
    }

    [RelayCommand]
    private void CloseInspectionDialog() => IsInspectionDialogOpen = false;

    [RelayCommand]
    private async Task RecordInspectionAsync()
    {
        if (NewEquipmentId == Guid.Empty) return;

        await ExecuteAsync(async () =>
        {
            var dto = new QualityRecordCreateDto
            {
                EquipmentId = NewEquipmentId,
                WorkOrderId = NewWorkOrderId,
                InspectionType = NewInspectionType,
                Result = NewResult,
                DefectType = NewDefectType,
                DefectDescription = NewDefectDescription,
                DefectCount = NewDefectCount,
                SampleSize = NewSampleSize,
                InspectorId = NewInspectorId,
                InspectorName = NewInspectorName,
                Notes = NewNotes
            };

            await _qualityService.RecordInspectionAsync(dto);
            IsInspectionDialogOpen = false;
            await LoadDataAsync();
        }, "Failed to record inspection");
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        InspectionTypeFilter = null;
        ResultFilter = null;
        CurrentPage = 1;
        _ = LoadQualityRecordsAsync();
    }

    partial void OnSearchTextChanged(string value) => _ = LoadQualityRecordsAsync();
    partial void OnInspectionTypeFilterChanged(InspectionType? value) => _ = LoadQualityRecordsAsync();
    partial void OnResultFilterChanged(InspectionResult? value) => _ = LoadQualityRecordsAsync();

    private static QualityDisplayItem MapToDisplayItem(QualityRecordDto record)
    {
        return new QualityDisplayItem
        {
            Id = record.Id,
            EquipmentId = record.EquipmentId,
            EquipmentCode = record.EquipmentCode,
            EquipmentName = record.EquipmentName,
            InspectionType = record.InspectionType,
            Result = record.Result,
            DefectType = record.DefectType,
            DefectCount = record.DefectCount,
            InspectedAt = record.InspectedAt,
            InspectorName = record.InspectorName
        };
    }
}

public partial class QualityDisplayItem : ObservableObject
{
    public Guid Id { get; set; }
    public Guid EquipmentId { get; set; }
    public string EquipmentCode { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public InspectionType InspectionType { get; set; }
    public InspectionResult Result { get; set; }
    public DefectType? DefectType { get; set; }
    public int? DefectCount { get; set; }
    public DateTime InspectedAt { get; set; }
    public string? InspectorName { get; set; }

    public string InspectionTypeDisplay => InspectionType.ToString();
    public string ResultDisplay => Result.ToString();
    public string DefectTypeDisplay => DefectType?.ToString() ?? "N/A";

    public string ResultColor => Result switch
    {
        InspectionResult.Pass => "#4CAF50",
        InspectionResult.Fail => "#F44336",
        InspectionResult.ConditionalPass => "#FF9800",
        _ => "#757575"
    };
}
