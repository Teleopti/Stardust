using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.GridBinding
{
	public class CurrentSelection<T>
	{
		private readonly ColumnEntityBinder<T> _columnEntityBinder;
		private readonly IList<T> _databoundEntities;
		private int _columnOffset;

		internal CurrentSelection(ColumnEntityBinder<T> columnEntityBinder, IList<T> databoundEntities)
		{
			_columnEntityBinder = columnEntityBinder;
			_databoundEntities = databoundEntities;
		}

		public IEnumerable<T> SelectedEntities()
		{
			GridRangeInfoList selectedRanges = getSelectedRanges();

			var uniqueEntities = new HashSet<T>();
			_columnOffset = _columnEntityBinder.ContentColumnOffset();
			foreach (GridRangeInfo selectedRange in selectedRanges)
			{
				addAllEntitiesIfRowSelected(selectedRange, uniqueEntities);
				addEntitiesFromSelectedColumns(selectedRange, uniqueEntities);
			}
			return uniqueEntities;
		}

		private void addEntitiesFromSelectedColumns(GridRangeInfo selectedRange, HashSet<T> uniqueEntities)
		{
			var adjustedRange = selectedRange.OffsetRange(0, -_columnOffset);
			for (int i = adjustedRange.Left; i <= adjustedRange.Right; i++)
			{
				uniqueEntities.Add(_databoundEntities[i]);
			}
		}

		private void addAllEntitiesIfRowSelected(GridRangeInfo selectedRange, HashSet<T> uniqueEntities)
		{
			if (AllEntitiesSelected(selectedRange))
			{
				foreach (T databoundEntity in _databoundEntities)
				{
					uniqueEntities.Add(databoundEntity);
				}
			}
		}

		private bool AllEntitiesSelected(GridRangeInfo selectedRange)
		{
			return selectedRange.IsRows || selectedRange.IsTable;
		}

		private GridRangeInfoList getSelectedRanges()
		{
			GridControl gridControl = _columnEntityBinder.Grid();
			return gridControl.Selections.Ranges;
		}
	}
}
