using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
	public interface IShiftStatisticsPresenter
	{
		void SetCellInfo(GridStyleInfo style, int rowIndex, int colIndex, object columnTag);
		int RowCount();
		void ReSort(int colIndex);
	}

	public class ShiftStatisticsPresenter : IShiftStatisticsPresenter
	{
		private readonly IShiftCategoryDistributionModel _model;

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

			var sortedCategories = _model.GetSortedShiftCategories();

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

			if (rowIndex > sortedCategories.Count)
			{
				if (colIndex < 4)
				{
					style.CellType = "IgnoreCellModel";
				}
				else
				{
					style.CellType = "NumericReadOnlyCell";
					style.CellValue = _model.GetSumOfDeviations();
				}
				
			}
		}

		private void setValues(GridStyleInfo style, int rowIndex, int colIndex, IList<IShiftCategory> sortedCategories)
		{
			var category = sortedCategories[rowIndex - 1];
			if (colIndex == 1 || colIndex == 2)
			{
				MinMax<int> minMax = _model.GetMinMaxForShiftCategory(category);
				style.CellType = "IntegerReadOnlyCell";
				style.CellValue = colIndex == 1 ? minMax.Minimum : minMax.Maximum;
			}
			else
			{
				style.CellType = "NumericReadOnlyCell";
				style.CellValue = colIndex == 3
					                  ? _model.GetAverageForShiftCategory(category)
					                  : _model.GetStandardDeviationForShiftCategory(category);
			}
		}

		private static void setRowHeader(GridStyleInfo style, int rowIndex, IList<IShiftCategory> sortedCategories)
		{
			if (rowIndex <= sortedCategories.Count)
			{
				style.Text = sortedCategories[rowIndex-1].Description.ShortName;
			}
			else
			{
				style.CellValue = "xxTotal";
			}
		}

		private static void setColumnHeader(GridStyleInfo style, int colIndex)
		{
			switch (colIndex)
			{
				case 1:
					style.CellValue = "xxMin";
					break;
				case 2:
					style.CellValue = "xxMax";
					break;
				case 3:
					style.CellValue = "xxAverage";
					break;
				case 4:
					style.CellValue = "xxStandardDeviation";
					break;
			}
		}

		public int RowCount()
		{
			//could be better
			return _model.GetSortedShiftCategories().Count + 1;
		}

		public void ReSort(int colIndex)
		{
			//switch (colIndex)
			//{
					
			//}
		}
	}
}