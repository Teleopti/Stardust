using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Windows.Controls.Primitives;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Scheduling.Requests;
using DataGrid = Microsoft.Windows.Controls.DataGrid;
using DataGridSortingEventArgs = Microsoft.Windows.Controls.DataGridSortingEventArgs;


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
            requestGrid.RowDetailsVisibilityMode = Microsoft.Windows.Controls.DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            requestGrid.RowDetailsVisibilityMode = Microsoft.Windows.Controls.DataGridRowDetailsVisibilityMode.Collapsed;
        }

        private void requestGrid_SelectedCellsChanged(object sender, Microsoft.Windows.Controls.SelectedCellsChangedEventArgs e)
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
            requestGrid.RowDetailsVisibilityMode = Microsoft.Windows.Controls.DataGridRowDetailsVisibilityMode.Collapsed;
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

        public void RemoveEvents()
        {
            requestGrid.SelectedCellsChanged -= requestGrid_SelectedCellsChanged;
            requestGrid.MouseDoubleClick -= requestGrid_MouseDoubleClick;
	        requestGrid.Sorting -= RequestGrid_OnSorting;
	        requestGrid.PreviewMouseDown -= RequestGrid_OnPreviewMouseDown;
        }

	    private void RequestGrid_OnSorting(object sender, DataGridSortingEventArgs e)
	    {
		    e.Handled = true;
	    }

	    private void RequestGrid_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
	    {
		    var dependencyObject = (DependencyObject) e.OriginalSource;
		    while (dependencyObject != null && !(dependencyObject is DataGridColumnHeader))
			    dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
		    if (dependencyObject == null || !(dependencyObject is DataGridColumnHeader) || !(sender is DataGrid)) return;

		    var header = dependencyObject as DataGridColumnHeader;
		    var view = CollectionViewSource.GetDefaultView((sender as DataGrid).ItemsSource);
		    var direction = ListSortDirection.Ascending;
		    if (view.SortDescriptions.Any(s => s.PropertyName == header.Column.SortMemberPath))
		    {
			    var previousSort = view.SortDescriptions.FirstOrDefault(s => s.PropertyName == header.Column.SortMemberPath);
			    direction = previousSort.Direction == ListSortDirection.Ascending
				                ? ListSortDirection.Descending
				                : ListSortDirection.Ascending;
			    view.SortDescriptions.Remove(previousSort);
		    }

		    var sortDescription = new SortDescription(header.Column.SortMemberPath, direction);
		    view.SortDescriptions.Insert(0, sortDescription);
	    }
    }
}
