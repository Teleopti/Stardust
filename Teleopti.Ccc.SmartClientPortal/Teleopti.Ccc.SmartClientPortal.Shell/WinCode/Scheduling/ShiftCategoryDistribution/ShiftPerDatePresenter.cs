using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ShiftCategoryDistribution
{
	public interface IShiftPerDatePresenter
	{
		void SetCellInfo(GridStyleInfo style, int rowIndex, int colIndex, object columnTag);
		int RowCount
		{
			get;
		}
		int ColumnCount();
		void ReSort(object tag, bool keepOrder);
	}

	public class ShiftPerDatePresenter : IShiftPerDatePresenter
	{
		private readonly IShiftCategoryDistributionModel _model;
		private IList<DateOnly> _sortedDates = new List<DateOnly>();
		private IShiftCategory _lastSortedCategory;
		private bool _lastSortOrderAscending = true;

		public ShiftPerDatePresenter(IShiftCategoryDistributionModel model)
		{
			_model = model;
		}

		public void SetCellInfo(GridStyleInfo style, int rowIndex, int colIndex, object columnTag)
		{
			if (colIndex == 0 && rowIndex == 0)
				return;

			if (rowIndex > _sortedDates.Count)
				return;

			if (colIndex > 0 && rowIndex == 0)
			{
				var shiftCategoryList = _model.GetSortedShiftCategories();
				if (colIndex > shiftCategoryList.Count)
					return;

				var categoryDescription = shiftCategoryList[colIndex - 1].Description;
				style.CellValue = categoryDescription.ShortName;
				style.CellTipText = categoryDescription.Name;
				style.Tag = shiftCategoryList[colIndex - 1];
			}

			if (colIndex == 0 && rowIndex > 0)
			{
				var dateOnly = _sortedDates[rowIndex - 1];
				style.CellValue = dateOnly.ToShortDateString();
				if (DateHelper.IsWeekend(dateOnly, TeleoptiPrincipal.CurrentPrincipal.Regional.Culture))
				{
					style.TextColor = ColorHelper.ScheduleViewBaseHolidayHeader;
				}
			}

			if (colIndex > 0 && rowIndex > 0)
			{
				if (!_model.ShouldUpdateViews) return;

				var dateOnly = _sortedDates[rowIndex - 1];
				style.CellType = "IntegerReadOnlyCell";
				var shiftCategory = columnTag as IShiftCategory;
				style.CellValue = shiftCategory == null ? 0 : _model.ShiftCategoryCount(dateOnly, shiftCategory);
				if (DateHelper.IsWeekend(dateOnly, TeleoptiPrincipal.CurrentPrincipal.Regional.Culture))
					style.BackColor = ColorHelper.ScheduleViewBaseHolidayCell;
			}
		}

		public int RowCount => _sortedDates.Count;

		public int ColumnCount()
		{
			return _model.GetSortedShiftCategories().Count;
		}

		public void ReSort(object tag, bool keepOrder)
		{
			var shiftCategory = tag as IShiftCategory;
			if (shiftCategory == null && _lastSortedCategory == null)
			{
				if(!keepOrder)
					_lastSortOrderAscending = !_lastSortOrderAscending;

				_sortedDates = _model.GetSortedDates(_lastSortOrderAscending);
				return;
			}

			if (shiftCategory == null && _lastSortedCategory != null)
			{
				_lastSortOrderAscending = !_lastSortOrderAscending;
				_sortedDates = _model.GetSortedDates(_lastSortOrderAscending);
				return;
			}

			if (shiftCategory == _lastSortedCategory)
			{
				_lastSortOrderAscending = !_lastSortOrderAscending;
				_sortedDates = _model.GetDatesSortedByNumberOfShiftCategories(shiftCategory, _lastSortOrderAscending);
				return;
			}

			if (shiftCategory != _lastSortedCategory)
			{
				_lastSortedCategory = shiftCategory;
				_sortedDates = _model.GetDatesSortedByNumberOfShiftCategories(shiftCategory, _lastSortOrderAscending);
				return;
			}

			_lastSortOrderAscending = !_lastSortOrderAscending;
			_lastSortedCategory = null;
			_sortedDates = _model.GetSortedDates(_lastSortOrderAscending);
		}
	}
}