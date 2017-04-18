using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ShiftCategoryDistribution
{
	public interface IShiftPerAgentPresenter
	{
		void SetCellInfo(GridStyleInfo style, int rowIndex, int colIndex, object columnTag);
		int RowCount { get; }
		int ColumnCount();
		void ReSort(object tag, bool keepOrder);
	}

	public class ShiftPerAgentPresenter : IShiftPerAgentPresenter
	{
		private readonly IShiftCategoryDistributionModel _model;
		private IList<IPerson> _sortedPersons = new List<IPerson>();
		private IShiftCategory _lastSortedCategory;
		private bool _lastSortOrderAscending = true;

		public ShiftPerAgentPresenter(IShiftCategoryDistributionModel model)
		{
			_model = model;
		}

		public void SetCellInfo(GridStyleInfo style, int rowIndex, int colIndex, object columnTag)
		{
			if (colIndex == 0 && rowIndex == 0)
				return;

			if (rowIndex > _sortedPersons.Count) 
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
				style.CellValue = _model.CommonAgentName(_sortedPersons[rowIndex - 1]);
			}

			if (colIndex > 0 && rowIndex > 0)
			{
				if (!_model.ShouldUpdateViews) return;
				var person = _sortedPersons[rowIndex - 1];
				style.CellType = "IntegerReadOnlyCell";
				var shiftCategory = columnTag as IShiftCategory;
				style.CellValue = shiftCategory == null ? 0 : _model.ShiftCategoryCount(person, shiftCategory);
			}
		}

		public int RowCount
		{
			get { return _sortedPersons.Count; }
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

				_sortedPersons = _model.GetSortedPersons(_lastSortOrderAscending);
				return;
			}

			if (shiftCategory == null && _lastSortedCategory != null)
			{
				_lastSortOrderAscending = !_lastSortOrderAscending;
				_sortedPersons = _model.GetSortedPersons(_lastSortOrderAscending);
				return;
			}

			if (shiftCategory == _lastSortedCategory)
			{
				_lastSortOrderAscending = !_lastSortOrderAscending;
				_sortedPersons = _model.GetAgentsSortedByNumberOfShiftCategories(shiftCategory, _lastSortOrderAscending);
				return;
			}

			if (shiftCategory != _lastSortedCategory)
			{
				_lastSortedCategory = shiftCategory;
				_sortedPersons = _model.GetAgentsSortedByNumberOfShiftCategories(shiftCategory, _lastSortOrderAscending);
				return;
			}

			_lastSortOrderAscending = !_lastSortOrderAscending;
			_lastSortedCategory = null;
			_sortedPersons = _model.GetSortedPersons(_lastSortOrderAscending);
		}
	}
}