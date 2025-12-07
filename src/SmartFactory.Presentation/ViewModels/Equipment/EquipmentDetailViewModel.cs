using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Interfaces;
using SmartFactory.Presentation.Services;
using SmartFactory.Presentation.ViewModels.Base;

namespace SmartFactory.Presentation.ViewModels.Equipment;

/// <summary>
/// ViewModel for equipment detail view.
/// </summary>
public partial class EquipmentDetailViewModel : PageViewModelBase
{
    private readonly IEquipmentRepository _equipmentRepository;

    [ObservableProperty]
    private Domain.Entities.Equipment? _equipment;

    [ObservableProperty]
    private ObservableCollection<Alarm> _recentAlarms = new();

    [ObservableProperty]
    private ObservableCollection<MaintenanceRecord> _maintenanceHistory = new();

    [ObservableProperty]
    private string _statusColorHex = "#607D8B";

    public EquipmentDetailViewModel(
        INavigationService navigationService,
        IEquipmentRepository equipmentRepository)
        : base(navigationService)
    {
        _equipmentRepository = equipmentRepository;
    }

    public override async void OnNavigatedTo(object? parameter)
    {
        if (parameter is Guid equipmentId)
        {
            await LoadEquipmentAsync(equipmentId);
        }
    }

    private async Task LoadEquipmentAsync(Guid id)
    {
        await ExecuteAsync(async () =>
        {
            Equipment = await _equipmentRepository.GetWithDetailsAsync(id);

            if (Equipment != null)
            {
                Title = Equipment.Name;
                RecentAlarms = new ObservableCollection<Alarm>(Equipment.Alarms);
                MaintenanceHistory = new ObservableCollection<MaintenanceRecord>(Equipment.MaintenanceRecords);

                // Set status color
                StatusColorHex = Equipment.Status switch
                {
                    Domain.Enums.EquipmentStatus.Running => "#4CAF50",
                    Domain.Enums.EquipmentStatus.Idle => "#2196F3",
                    Domain.Enums.EquipmentStatus.Warning => "#FF9800",
                    Domain.Enums.EquipmentStatus.Error => "#F44336",
                    Domain.Enums.EquipmentStatus.Maintenance => "#9C27B0",
                    _ => "#607D8B"
                };
            }
        });
    }

    [RelayCommand]
    private void GoBack()
    {
        NavigationService.GoBack();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (Equipment != null)
        {
            await LoadEquipmentAsync(Equipment.Id);
        }
    }
}
