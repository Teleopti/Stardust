﻿

using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
	public interface IShiftPerDayPresenter
	{
		void SetCellInfo(GridStyleInfo style, int rowIndex, int colIndex, object columnTag);
		int RowCount
		{
			get;
		}
		int ColumnCount();
		void ReSort(object tag, bool keepOrder);
	}

	public class ShiftPerDayPresenter : IShiftPerDayPresenter
	{
		private readonly IShiftCategoryDistributionModel _model;
		private IList<DateOnly> _sortedDates = new List<DateOnly>();
		private IShiftCategory _lastSortedCategory;
		private bool _lastSortOrderAscending = true;

		public ShiftPerDayPresenter(IShiftCategoryDistributionModel model)
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
				style.CellValue = shiftCategoryList[colIndex - 1].Description.ShortName;
				style.Tag = shiftCategoryList[colIndex - 1];
			}

			if (colIndex == 0 && rowIndex > 0)
			{
				var dateOnly = _sortedDates[rowIndex - 1];
				style.CellValue = dateOnly.ToShortDateString();
				if (DateHelper.IsWeekend(dateOnly, TeleoptiPrincipal.Current.Regional.Culture))
				{
					style.TextColor = ColorHelper.ScheduleViewBaseHolidayHeader;
				}
			}

			if (colIndex > 0 && rowIndex > 0)
			{
				var dateOnly = _sortedDates[rowIndex - 1];
				style.CellType = "IntegerReadOnlyCell";
				var shiftCategory = columnTag as IShiftCategory;
				style.CellValue = shiftCategory == null ? 0 : _model.ShiftCategoryCount(dateOnly, shiftCategory);
				if (DateHelper.IsWeekend(dateOnly, TeleoptiPrincipal.Current.Regional.Culture))
					style.BackColor = ColorHelper.ScheduleViewBaseHolidayCell;
			}
		}

		public int RowCount
		{
			get
			{
				return _sortedDates.Count;
			}
		}

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