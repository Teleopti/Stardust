using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Windows.Controls;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Scheduling.Requests;

namespace Teleopti.Ccc.WpfControls.Controls.Requests.Views
{
    /// <summary>
    /// Interaction logic for HandlePersonRequestView.xaml
    /// </summary>
    public partial class HandlePersonRequestView
    {

    	public HandlePersonRequestView()
        {
            InitializeComponent();
        }

        //Sorry Hank, had to put all this shit here to make this expand/collapse thing work Teleopti style
        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            requestGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            requestGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
        }

        private void requestGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            var dataContext = requestGrid.DataContext as HandlePersonRequestViewModel;
            var selectedItems = 0;
            if (requestGrid.SelectedCells != null)
                selectedItems = requestGrid.SelectedCells.Count / requestGrid.Columns.Count;
            if (dataContext != null && dataContext.SelectedModels != null)
                if (dataContext.SelectedModels.Count != selectedItems)
                {
                    var i = 0;
                    foreach (var model in dataContext.SelectedModels)
                    {
                        model.IsSelected = i < selectedItems;
                        i++;
                    }
                }
            requestGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
        }

        private void Expander_LostFocus(object sender, RoutedEventArgs e)
        {
            var expander = sender as Expander;
            if (expander != null) expander.IsExpanded = false;
        }

        private void requestGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var src = VisualTreeHelper.GetParent((DependencyObject)e.OriginalSource);
            var srcType = src.GetType();
            if (srcType != typeof (ListViewItem) && srcType != typeof (ContentPresenter)) return;
            var dataContext = requestGrid.DataContext as HandlePersonRequestViewModel;
            if (dataContext != null)
                dataContext.ShowRequestDetailsView();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            string helpContext = HelpProvider.GetHelpString(this);
            if (string.IsNullOrEmpty(helpContext) && Parent == null) return;
            var elementParent = Parent as FrameworkElement;
            if (elementParent == null || elementParent.Parent == null) return;
            HelpProvider.SetHelpString(elementParent.Parent, helpContext);
        }
    }
}