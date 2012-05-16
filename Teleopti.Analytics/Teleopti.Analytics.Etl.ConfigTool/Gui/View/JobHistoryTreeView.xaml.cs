using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Teleopti.Analytics.Etl.ConfigTool.Gui.ViewModel;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.View
{
	/// <summary>
	/// Interaction logic for JobHistoryTreeView.xaml
	/// </summary>
	public partial class JobHistoryTreeView : UserControl
	{
		public JobHistoryTreeView()
		{
			InitializeComponent();
		}

		public void LoadData(DateTime startDate, DateTime endDate, Guid businessUnitId, bool showOnlyErrors)
		{
			DataContext = null;
			DataContext = JobHistoryMapper.Map(startDate, endDate, businessUnitId, showOnlyErrors);
		}

		private void treeListView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

			if (treeViewItem != null)
			{
				treeViewItem.IsSelected = true;
				e.Handled = true;
			}
		}

		private static TreeViewItem VisualUpwardSearch(DependencyObject source)
		{
			while (source != null && !(source is TreeViewItem))
				source = VisualTreeHelper.GetParent(source);

			return source as TreeViewItem;
		}

		private void treeListView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			var treeView = sender as TreeView;
			if (treeView != null && treeView.SelectedItem != null)
			{
				((JobHistoryTreeViewModel)DataContext).SelectedItem = (IJobHistory)treeView.SelectedItem;
				e.Handled = true;
			}
		}

		private void ContextMenu_Opened(object sender, RoutedEventArgs e)
		{
			var menu = sender as ContextMenu;
			if (menu != null) menu.DataContext = DataContext;
		}
	}
}
