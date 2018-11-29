using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Teleopti.Analytics.Etl.Common.Entity;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.ConfigTool.Gui.ViewModel;
using Teleopti.Ccc.Domain.FeatureFlags;

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

		public void LoadData(DateTime startDate, DateTime endDate, BusinessUnitItem businessUnit, bool showOnlyErrors)
		{
			DataContext = null;
			DataContext = JobHistoryMapper.Map(startDate, endDate, businessUnit.Id, showOnlyErrors);
		}

		private void treeListView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			var treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

			if (treeViewItem == null) return;
			treeViewItem.IsSelected = true;
			e.Handled = true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		private static TreeViewItem VisualUpwardSearch(DependencyObject source)
		{
			while (source != null && !(source is TreeViewItem))
				source = VisualTreeHelper.GetParent(source);

			return source as TreeViewItem;
		}

		private void treeListView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			if (!(sender is TreeView treeView) || !(DataContext is JobHistoryTreeViewModel model) ||
				treeView.SelectedItem == null || !(treeView.SelectedItem is IJobHistory item)) return;

			model.SelectedItem = item;
			e.Handled = true;
		}

		private void ContextMenu_Opened(object sender, RoutedEventArgs e)
		{
			if (sender is ContextMenu menu) menu.DataContext = DataContext;
		}
	}
}
