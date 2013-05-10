﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Scheduling.Requests;
using DataGrid = Microsoft.Windows.Controls.DataGrid;
using DataGridRowDetailsVisibilityMode = Microsoft.Windows.Controls.DataGridRowDetailsVisibilityMode;
using DataGridSortingEventArgs = Microsoft.Windows.Controls.DataGridSortingEventArgs;
using SelectedCellsChangedEventArgs = Microsoft.Windows.Controls.SelectedCellsChangedEventArgs;


namespace Teleopti.Ccc.WpfControls.Controls.Requests.Views
{
    /// <summary>
    /// Interaction logic for HandlePersonRequestView.xaml
    /// </summary>
    public partial class HandlePersonRequestView
    {
		private readonly IList<SortDescription> _sortDirections;
    
		public HandlePersonRequestView()
        {
            InitializeComponent();
			_sortDirections = new List<SortDescription>();
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
		    while (dependencyObject != null && !(dependencyObject is Microsoft.Windows.Controls.Primitives.DataGridColumnHeader))
			    dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
			if (dependencyObject == null || !(dependencyObject is Microsoft.Windows.Controls.Primitives.DataGridColumnHeader) || !(sender is DataGrid)) return;
		    if (requestGrid.Items.IsEmpty)
			    return;

			var header = dependencyObject as Microsoft.Windows.Controls.Primitives.DataGridColumnHeader;
		    var direction = ListSortDirection.Ascending;
		    if (_sortDirections.Any(s => s.PropertyName == header.Column.SortMemberPath))
		    {
			    var previousSort = _sortDirections.FirstOrDefault(s => s.PropertyName == header.Column.SortMemberPath);
			    direction = previousSort.Direction == ListSortDirection.Ascending
				                ? ListSortDirection.Descending
				                : ListSortDirection.Ascending;
			    _sortDirections.Remove(previousSort);
		    }
		    _sortDirections.Insert(0, new SortDescription(header.Column.SortMemberPath, direction));
		    var model = DataContext as HandlePersonRequestViewModel;
			if (model != null)
				model.SortSourceList(_sortDirections);
			requestGrid.SelectedItem = null;
		    e.Handled = true;
	    }
    }
}
