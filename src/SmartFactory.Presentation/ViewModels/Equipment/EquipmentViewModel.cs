using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;
using SmartFactory.Presentation.Services;
using SmartFactory.Presentation.ViewModels.Base;
using SmartFactory.Presentation.Views.Equipment;

namespace SmartFactory.Presentation.ViewModels.Equipment;

/// <summary>
/// ViewModel for the equipment list view.
/// </summary>
public partial class EquipmentViewModel : PageViewModelBase
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IFactoryContextService _factoryContext;

    [ObservableProperty]
    private ObservableCollection<EquipmentDisplayItem> _equipment = new();

    [ObservableProperty]
    private EquipmentDisplayItem? _selectedEquipment;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private EquipmentStatus? _statusFilter;

    [ObservableProperty]
    private ObservableCollection<EquipmentStatus> _statusOptions = new();

    public EquipmentViewModel(
        INavigationService navigationService,
        IEquipmentRepository equipmentRepository,
        IFactoryContextService factoryContext)
        : base(navigationService)
    {
        Title = "Equipment Management";
        _equipmentRepository = equipmentRepository;
        _factoryContext = factoryContext;

        // Initialize status filter options
        StatusOptions = new ObservableCollection<EquipmentStatus>(
            Enum.GetValues<EquipmentStatus>());

        _factoryContext.CurrentFactoryChanged += (s, f) => _ = LoadEquipmentAsync();
    }

    public override async void OnNavigatedTo(object? parameter)
    {
        await LoadEquipmentAsync();
    }

    [RelayCommand]
    private async Task LoadEquipmentAsync()
    {
        await ExecuteAsync(async () =>
        {
            var factoryId = _factoryContext.CurrentFactoryId;
            if (!factoryId.HasValue) return;

            var equipment = await _equipmentRepository.GetByFactoryAsync(factoryId.Value);

            var filteredEquipment = equipment.AsEnumerable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLowerInvariant();
                filteredEquipment = filteredEquipment.Where(e =>
                    e.Name.ToLowerInvariant().Contains(searchLower) ||
                    e.Code.ToLowerInvariant().Contains(searchLower));
            }

            // Apply status filter
            if (StatusFilter.HasValue)
            {
                filteredEquipment = filteredEquipment.Where(e => e.Status == StatusFilter.Value);
            }

            Equipment = new ObservableCollection<EquipmentDisplayItem>(
                filteredEquipment.Select(e => new EquipmentDisplayItem
                {
                    Id = e.Id,
                    Code = e.Code,
                    Name = e.Name,
                    Type = e.Type.ToString(),
                    Status = e.Status,
                    ProductionLineName = e.ProductionLine.Name,
                    LastHeartbeat = e.LastHeartbeat,
                    IsOnline = e.IsOnline
                }));
        });
    }

    [RelayCommand]
    private void ViewDetails(EquipmentDisplayItem? equipment)
    {
        if (equipment != null)
        {
            NavigationService.NavigateTo(typeof(EquipmentDetailView), equipment.Id);
        }
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        StatusFilter = null;
        _ = LoadEquipmentAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = LoadEquipmentAsync();
    }

    partial void OnStatusFilterChanged(EquipmentStatus? value)
    {
        _ = LoadEquipmentAsync();
    }
}

/// <summary>
/// Display model for equipment in the list.
/// </summary>
public partial class EquipmentDisplayItem : ObservableObject
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public EquipmentStatus Status { get; set; }
    public string ProductionLineName { get; set; } = string.Empty;
    public DateTime? LastHeartbeat { get; set; }
    public bool IsOnline { get; set; }

    public string StatusDisplay => Status.ToString();
}
