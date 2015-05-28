using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.SyncfusionGridBinding
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
			GridRangeInfoList selectedRanges = GetSelectedRanges();

			HashSet<T> uniqueEntities = new HashSet<T>();
			_columnOffset = _columnEntityBinder.ContentColumnOffset();
			foreach (GridRangeInfo selectedRange in selectedRanges)
			{
				AddAllEntitiesIfRowSelected(selectedRange, uniqueEntities);
				AddEntitiesFromSelectedColumns(selectedRange, uniqueEntities);
			}
			return uniqueEntities;
		}

		private void AddEntitiesFromSelectedColumns(GridRangeInfo selectedRange, HashSet<T> uniqueEntities)
		{
			var adjustedRange = selectedRange.OffsetRange(0, -_columnOffset);
			for (int i = adjustedRange.Left; i <= adjustedRange.Right; i++)
			{
				uniqueEntities.Add(_databoundEntities[i]);
			}
		}

		private void AddAllEntitiesIfRowSelected(GridRangeInfo selectedRange, HashSet<T> uniqueEntities)
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

		private GridRangeInfoList GetSelectedRanges()
		{
			GridControl gridControl = _columnEntityBinder.Grid();
			return gridControl.Selections.Ranges;
		}
	}
}
