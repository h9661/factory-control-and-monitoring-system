using System.Windows.Controls;
using SmartFactory.Presentation.ViewModels.FloorPlan;

namespace SmartFactory.Presentation.Views.FloorPlan;

/// <summary>
/// Interaction logic for FloorPlanView.xaml
/// </summary>
public partial class FloorPlanView : UserControl
{
    public FloorPlanView(FloorPlanViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        Loaded += async (s, e) =>
        {
            if (DataContext is FloorPlanViewModel vm)
            {
                await vm.LoadAsync();
            }
        };

        Unloaded += async (s, e) =>
        {
            if (DataContext is FloorPlanViewModel vm)
            {
                await vm.UnloadAsync();
            }
        };
    }
}
