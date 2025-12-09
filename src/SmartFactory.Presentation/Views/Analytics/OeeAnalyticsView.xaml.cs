using System.Windows.Controls;
using SmartFactory.Presentation.ViewModels.Analytics;

namespace SmartFactory.Presentation.Views.Analytics;

/// <summary>
/// Interaction logic for OeeAnalyticsView.xaml
/// </summary>
public partial class OeeAnalyticsView : UserControl
{
    public OeeAnalyticsView(OeeAnalyticsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
