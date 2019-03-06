using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ShiftCategoryDistribution
{
	public interface IShiftStatisticsPresenter
	{
		void SetCellInfo(GridStyleInfo style, int rowIndex, int colIndex, object columnTag);
		int RowCount();
		void ReSort(int colIndex, bool keepOrder);
	}

	public class ShiftStatisticsPresenter : IShiftStatisticsPresenter
	{
		private readonly IShiftCategoryDistributionModel _model;
		private int _lastSortColumn = 0;
		private bool _lastSortOrderAscending = true;

		public ShiftStatisticsPresenter(IShiftCategoryDistributionModel model)
		{
			_model = model;
		}

		public void SetCellInfo(GridStyleInfo style, int rowIndex, int colIndex, object columnTag)
		{
			
			if (colIndex == 0 && rowIndex == 0)
				return;

			if (colIndex > 0 && rowIndex == 0)
			{
				setColumnHeader(style, colIndex);
				return;
			}

			var sortedCategories = new List<IShiftCategory>();
			sortedCategories = _model.GetSortedShiftCategories().ToList();

			if (_lastSortColumn == 0)
			{
				if (_lastSortOrderAscending)
					sortedCategories.Reverse();
			}
			
			if(_lastSortColumn == 1)
				sortedCategories = _model.GetShiftCategoriesSortedByMinMax(_lastSortOrderAscending, true).ToList();
			if(_lastSortColumn == 2)
				sortedCategories = _model.GetShiftCategoriesSortedByMinMax(_lastSortOrderAscending, false).ToList();
			if (_lastSortColumn == 3)
				sortedCategories = _model.GetShiftCategoriesSortedByAverage(_lastSortOrderAscending).ToList();
			//if (_lastSortColumn == 4)
			//	sortedCategories = _model.GetShiftCategoriesSortedByStandardDeviation(_lastSortOrderAscending).ToList();

			if (colIndex == 0 && rowIndex > 0)
			{
				setRowHeader(style, rowIndex, sortedCategories);
				return;
			}

			if (rowIndex <= sortedCategories.Count && rowIndex > 0)
			{
				setValues(style, rowIndex, colIndex, sortedCategories);
				return;
			}

			//if (rowIndex > sortedCategories.Count)
			//{
			//	if (colIndex < 4)
			//	{
			//		style.CellType = "IgnoreCellModel";
			//	}
			//	else
			//	{
			//		if (!_model.ShouldUpdateViews) return;
			//		style.CellType = "NumericReadOnlyCell";
			//		style.CellValue = _model.GetSumOfDeviations();
			//	}
				
			//}
		}

		private void setValues(GridStyleInfo style, int rowIndex, int colIndex, IList<IShiftCategory> sortedCategories)
		{
			if (!_model.ShouldUpdateViews) return;

			var category = sortedCategories[rowIndex - 1];
			if (colIndex == 1 || colIndex == 2)
			{
				var tempCellValue = style.CellValue;
				MinMax<int> minMax = _model.GetMinMaxForShiftCategory(category);
				style.CellType = "IntegerReadOnlyCell";
				style.CellValue = colIndex == 1 ? minMax.Minimum : minMax.Maximum;
				if(style.CellValue != tempCellValue)
					_model.OnChartUpdateNeeded();
			}
			if (colIndex == 3)
			{
				style.CellType = "NumericReadOnlyCell";
				style.CellValue = _model.GetAverageForShiftCategory(category, _model.GetSortedPersons(false));
				//style.CellValue = colIndex == 3
				//					  ? _model.GetAverageForShiftCategory(category)
				//					  : _model.GetStandardDeviationForShiftCategory(category);
			}
		}

		private static void setRowHeader(GridStyleInfo style, int rowIndex, IList<IShiftCategory> sortedCategories)
		{
			if (rowIndex <= sortedCategories.Count)
			{
				var categoryDescription = sortedCategories[rowIndex - 1].Description;
				style.CellValue = categoryDescription.ShortName;
				style.CellTipText = categoryDescription.Name;
			}
			//else
			//{
			//	style.CellValue = UserTexts.Resources.Total;
			//}
		}

		private static void setColumnHeader(GridStyleInfo style, int colIndex)
		{
			switch (colIndex)
			{
				case 1:
					style.CellValue = UserTexts.Resources.Min;
					break;
				case 2:
					style.CellValue = UserTexts.Resources.Max;
					break;
				case 3:
					style.CellValue = UserTexts.Resources.Average;
					break;
				//case 4:
				//	style.CellValue = UserTexts.Resources.StandardDeviation;
				//	break;
			}
		}

		public int RowCount()
		{
			//could be better
			return _model.GetSortedShiftCategories().Count;
		}

		public void ReSort(int colIndex, bool keepOrder)
		{
			_lastSortColumn = colIndex;

			if(!keepOrder)
				_lastSortOrderAscending = !_lastSortOrderAscending;
		}
	}
}