using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Teleopti.Ccc.WinCode.Converters;

namespace Teleopti.Ccc.WpfControls.Converters
{
	public class CustomSortBehaviour
	{
		public static readonly DependencyProperty CustomSorterProperty =
			DependencyProperty.RegisterAttached("CustomSorter", typeof(ICustomSorter), typeof(DataGridColumn));

		public static ICustomSorter GetCustomSorter(DataGridColumn gridColumn)
		{
			return (ICustomSorter)gridColumn.GetValue(CustomSorterProperty);
		}

		public static void SetCustomSorter(DataGridColumn gridColumn, ICustomSorter value)
		{
			gridColumn.SetValue(CustomSorterProperty, value);
		}


		public static readonly DependencyProperty AllowCustomSortProperty =
			DependencyProperty.RegisterAttached("AllowCustomSort", typeof(bool),
			                                    typeof(DataGrid), new UIPropertyMetadata(false, onAllowCustomSortChanged));

		public static bool GetAllowCustomSort(DataGrid grid)
		{
			return (bool)grid.GetValue(AllowCustomSortProperty);
		}

		public static void SetAllowCustomSort(DataGrid grid, bool value)
		{
			grid.SetValue(AllowCustomSortProperty, value);
		}

		private static void onAllowCustomSortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var existing = d as DataGrid;
			if (existing == null) return;

			var oldAllow = (bool)e.OldValue;
			var newAllow = (bool)e.NewValue;

			if (!oldAllow && newAllow)
			{
				existing.Sorting += handleCustomSorting;
			}
			else
			{
				existing.Sorting -= handleCustomSorting;
			}
		}

		private static void handleCustomSorting(object sender, DataGridSortingEventArgs e)
		{
			var dataGrid = sender as DataGrid;
			if (dataGrid == null || !GetAllowCustomSort(dataGrid)) return;

			var listColView = dataGrid.ItemsSource as ListCollectionView;
			if (listColView == null)
			{
				listColView = (ListCollectionView)CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
			}

			var sorter = GetCustomSorter(e.Column);
			if (sorter == null) return;

			e.Handled = true;
			var direction = (e.Column.SortDirection != ListSortDirection.Ascending)
				                ? ListSortDirection.Ascending
				                : ListSortDirection.Descending;

			e.Column.SortDirection = sorter.SortDirection = direction;
			listColView.CustomSort = sorter;
		}
	}
}