using System.Windows.Controls;
using SmartFactory.Presentation.ViewModels.Maintenance;

namespace SmartFactory.Presentation.Views.Maintenance;

/// <summary>
/// Interaction logic for PredictiveMaintenanceView.xaml
/// </summary>
public partial class PredictiveMaintenanceView : UserControl
{
    public PredictiveMaintenanceView(PredictiveMaintenanceViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
