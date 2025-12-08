using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartFactory.Application.DTOs.Common;
using SmartFactory.Application.DTOs.WorkOrder;
using SmartFactory.Application.Interfaces;
using SmartFactory.Domain.Enums;
using SmartFactory.Presentation.Services;
using SmartFactory.Presentation.ViewModels.Base;

namespace SmartFactory.Presentation.ViewModels.Production;

/// <summary>
/// ViewModel for the production/work order management view.
/// </summary>
public partial class ProductionViewModel : PageViewModelBase
{
    private readonly IWorkOrderService _workOrderService;
    private readonly IFactoryContextService _factoryContext;

    [ObservableProperty]
    private ObservableCollection<WorkOrderDisplayItem> _workOrders = new();

    [ObservableProperty]
    private WorkOrderDisplayItem? _selectedWorkOrder;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private WorkOrderStatus? _statusFilter;

    [ObservableProperty]
    private WorkOrderPriority? _priorityFilter;

    [ObservableProperty]
    private ObservableCollection<WorkOrderStatus> _statusOptions = new();

    [ObservableProperty]
    private ObservableCollection<WorkOrderPriority> _priorityOptions = new();

    [ObservableProperty]
    private ProductionSummaryDto? _productionSummary;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _pageSize = 20;

    // Create Work Order Dialog
    [ObservableProperty]
    private bool _isCreateDialogOpen;

    [ObservableProperty]
    private string _newOrderNumber = string.Empty;

    [ObservableProperty]
    private string _newProductCode = string.Empty;

    [ObservableProperty]
    private string _newProductName = string.Empty;

    [ObservableProperty]
    private int _newTargetQuantity = 100;

    [ObservableProperty]
    private DateTime _newScheduledStart = DateTime.Today;

    [ObservableProperty]
    private DateTime _newScheduledEnd = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private WorkOrderPriority _newPriority = WorkOrderPriority.Normal;

    // Progress Dialog
    [ObservableProperty]
    private bool _isProgressDialogOpen;

    [ObservableProperty]
    private int _progressCompletedQuantity;

    [ObservableProperty]
    private int _progressDefectQuantity;

    // Cancel Dialog
    [ObservableProperty]
    private bool _isCancelDialogOpen;

    [ObservableProperty]
    private string _cancelReason = string.Empty;

    public ProductionViewModel(
        INavigationService navigationService,
        IWorkOrderService workOrderService,
        IFactoryContextService factoryContext)
        : base(navigationService)
    {
        Title = "Production Planning";
        _workOrderService = workOrderService;
        _factoryContext = factoryContext;

        // Initialize filter options
        StatusOptions = new ObservableCollection<WorkOrderStatus>(Enum.GetValues<WorkOrderStatus>());
        PriorityOptions = new ObservableCollection<WorkOrderPriority>(Enum.GetValues<WorkOrderPriority>());

        _factoryContext.CurrentFactoryChanged += (s, f) => _ = LoadWorkOrdersAsync();
    }

    public override async void OnNavigatedTo(object? parameter)
    {
        await LoadWorkOrdersAsync();
        await LoadProductionSummaryAsync();
    }

    [RelayCommand]
    private async Task LoadWorkOrdersAsync()
    {
        await ExecuteAsync(async () =>
        {
            var filter = new WorkOrderFilterDto
            {
                FactoryId = _factoryContext.CurrentFactoryId,
                Status = StatusFilter,
                Priority = PriorityFilter,
                SearchText = SearchText
            };

            var pagination = new PaginationDto
            {
                PageNumber = CurrentPage,
                PageSize = PageSize
            };

            var result = await _workOrderService.GetWorkOrdersAsync(filter, pagination);

            WorkOrders = new ObservableCollection<WorkOrderDisplayItem>(
                result.Items.Select(w => MapToDisplayItem(w)));

            TotalPages = result.TotalPages;
        });
    }

    [RelayCommand]
    private async Task LoadProductionSummaryAsync()
    {
        await ExecuteAsync(async () =>
        {
            ProductionSummary = await _workOrderService.GetProductionSummaryAsync(
                _factoryContext.CurrentFactoryId,
                DateTime.Today);
        });
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadWorkOrdersAsync();
        await LoadProductionSummaryAsync();
    }

    [RelayCommand]
    private void ShowCreateDialog()
    {
        // Generate order number
        NewOrderNumber = $"WO-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        NewProductCode = string.Empty;
        NewProductName = string.Empty;
        NewTargetQuantity = 100;
        NewScheduledStart = DateTime.Today;
        NewScheduledEnd = DateTime.Today.AddDays(1);
        NewPriority = WorkOrderPriority.Normal;
        IsCreateDialogOpen = true;
    }

    [RelayCommand]
    private void CloseCreateDialog()
    {
        IsCreateDialogOpen = false;
    }

    [RelayCommand]
    private async Task CreateWorkOrderAsync()
    {
        if (!_factoryContext.CurrentFactoryId.HasValue) return;

        await ExecuteAsync(async () =>
        {
            var dto = new WorkOrderCreateDto
            {
                FactoryId = _factoryContext.CurrentFactoryId.Value,
                OrderNumber = NewOrderNumber,
                ProductCode = NewProductCode,
                ProductName = NewProductName,
                TargetQuantity = NewTargetQuantity,
                ScheduledStart = NewScheduledStart,
                ScheduledEnd = NewScheduledEnd,
                Priority = NewPriority
            };

            await _workOrderService.CreateWorkOrderAsync(dto);
            IsCreateDialogOpen = false;
            await LoadWorkOrdersAsync();
            await LoadProductionSummaryAsync();
        }, "Failed to create work order");
    }

    [RelayCommand]
    private async Task StartWorkOrderAsync(WorkOrderDisplayItem? workOrder)
    {
        if (workOrder == null) return;

        await ExecuteAsync(async () =>
        {
            await _workOrderService.StartWorkOrderAsync(workOrder.Id);
            await LoadWorkOrdersAsync();
            await LoadProductionSummaryAsync();
        }, "Failed to start work order");
    }

    [RelayCommand]
    private async Task PauseWorkOrderAsync(WorkOrderDisplayItem? workOrder)
    {
        if (workOrder == null) return;

        await ExecuteAsync(async () =>
        {
            await _workOrderService.PauseWorkOrderAsync(workOrder.Id);
            await LoadWorkOrdersAsync();
        }, "Failed to pause work order");
    }

    [RelayCommand]
    private async Task ResumeWorkOrderAsync(WorkOrderDisplayItem? workOrder)
    {
        if (workOrder == null) return;

        await ExecuteAsync(async () =>
        {
            await _workOrderService.ResumeWorkOrderAsync(workOrder.Id);
            await LoadWorkOrdersAsync();
        }, "Failed to resume work order");
    }

    [RelayCommand]
    private async Task CompleteWorkOrderAsync(WorkOrderDisplayItem? workOrder)
    {
        if (workOrder == null) return;

        await ExecuteAsync(async () =>
        {
            await _workOrderService.CompleteWorkOrderAsync(workOrder.Id);
            await LoadWorkOrdersAsync();
            await LoadProductionSummaryAsync();
        }, "Failed to complete work order");
    }

    [RelayCommand]
    private void ShowProgressDialog(WorkOrderDisplayItem? workOrder)
    {
        if (workOrder == null) return;
        SelectedWorkOrder = workOrder;
        ProgressCompletedQuantity = workOrder.CompletedQuantity;
        ProgressDefectQuantity = workOrder.DefectQuantity;
        IsProgressDialogOpen = true;
    }

    [RelayCommand]
    private void CloseProgressDialog()
    {
        IsProgressDialogOpen = false;
    }

    [RelayCommand]
    private async Task ReportProgressAsync()
    {
        if (SelectedWorkOrder == null) return;

        await ExecuteAsync(async () =>
        {
            var dto = new WorkOrderProgressDto
            {
                CompletedQuantity = ProgressCompletedQuantity,
                DefectQuantity = ProgressDefectQuantity
            };

            await _workOrderService.ReportProgressAsync(SelectedWorkOrder.Id, dto);
            IsProgressDialogOpen = false;
            await LoadWorkOrdersAsync();
            await LoadProductionSummaryAsync();
        }, "Failed to report progress");
    }

    [RelayCommand]
    private void ShowCancelDialog(WorkOrderDisplayItem? workOrder)
    {
        if (workOrder == null) return;
        SelectedWorkOrder = workOrder;
        CancelReason = string.Empty;
        IsCancelDialogOpen = true;
    }

    [RelayCommand]
    private void CloseCancelDialog()
    {
        IsCancelDialogOpen = false;
    }

    [RelayCommand]
    private async Task CancelWorkOrderAsync()
    {
        if (SelectedWorkOrder == null || string.IsNullOrWhiteSpace(CancelReason)) return;

        await ExecuteAsync(async () =>
        {
            await _workOrderService.CancelWorkOrderAsync(SelectedWorkOrder.Id, CancelReason);
            IsCancelDialogOpen = false;
            await LoadWorkOrdersAsync();
            await LoadProductionSummaryAsync();
        }, "Failed to cancel work order");
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        StatusFilter = null;
        PriorityFilter = null;
        CurrentPage = 1;
        _ = LoadWorkOrdersAsync();
    }

    [RelayCommand]
    private async Task GoToPreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadWorkOrdersAsync();
        }
    }

    [RelayCommand]
    private async Task GoToNextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadWorkOrdersAsync();
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        CurrentPage = 1;
        _ = LoadWorkOrdersAsync();
    }

    partial void OnStatusFilterChanged(WorkOrderStatus? value)
    {
        CurrentPage = 1;
        _ = LoadWorkOrdersAsync();
    }

    partial void OnPriorityFilterChanged(WorkOrderPriority? value)
    {
        CurrentPage = 1;
        _ = LoadWorkOrdersAsync();
    }

    private static WorkOrderDisplayItem MapToDisplayItem(WorkOrderDto workOrder)
    {
        return new WorkOrderDisplayItem
        {
            Id = workOrder.Id,
            OrderNumber = workOrder.OrderNumber,
            ProductCode = workOrder.ProductCode,
            ProductName = workOrder.ProductName,
            Status = workOrder.Status,
            Priority = workOrder.Priority,
            TargetQuantity = workOrder.TargetQuantity,
            CompletedQuantity = workOrder.CompletedQuantity,
            DefectQuantity = workOrder.DefectQuantity,
            ScheduledStart = workOrder.ScheduledStart,
            ScheduledEnd = workOrder.ScheduledEnd,
            ActualStart = workOrder.ActualStart,
            ActualEnd = workOrder.ActualEnd,
            YieldRate = workOrder.YieldRate
        };
    }
}

/// <summary>
/// Display model for work orders in the list.
/// </summary>
public partial class WorkOrderDisplayItem : ObservableObject
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public WorkOrderStatus Status { get; set; }
    public WorkOrderPriority Priority { get; set; }
    public int TargetQuantity { get; set; }
    public int CompletedQuantity { get; set; }
    public int DefectQuantity { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public DateTime? ActualStart { get; set; }
    public DateTime? ActualEnd { get; set; }
    public double YieldRate { get; set; }

    public string StatusDisplay => Status.ToString();
    public string PriorityDisplay => Priority.ToString();

    public double ProgressPercentage => TargetQuantity > 0
        ? Math.Min(100, (double)CompletedQuantity / TargetQuantity * 100)
        : 0;

    public string StatusColor => Status switch
    {
        WorkOrderStatus.Draft => "#9E9E9E",
        WorkOrderStatus.Scheduled => "#03A9F4",
        WorkOrderStatus.InProgress => "#2196F3",
        WorkOrderStatus.Paused => "#FF9800",
        WorkOrderStatus.Completed => "#4CAF50",
        WorkOrderStatus.Cancelled => "#F44336",
        WorkOrderStatus.OnHold => "#795548",
        _ => "#757575"
    };

    public string PriorityColor => Priority switch
    {
        WorkOrderPriority.Low => "#9E9E9E",
        WorkOrderPriority.Normal => "#2196F3",
        WorkOrderPriority.High => "#FF9800",
        WorkOrderPriority.Urgent => "#FF5722",
        WorkOrderPriority.Critical => "#F44336",
        _ => "#757575"
    };

    public bool CanStart => Status == WorkOrderStatus.Scheduled || Status == WorkOrderStatus.Draft;
    public bool CanPause => Status == WorkOrderStatus.InProgress;
    public bool CanResume => Status == WorkOrderStatus.Paused;
    public bool CanComplete => Status == WorkOrderStatus.InProgress;
    public bool CanCancel => Status != WorkOrderStatus.Completed && Status != WorkOrderStatus.Cancelled;
    public bool CanReportProgress => Status == WorkOrderStatus.InProgress;
}
