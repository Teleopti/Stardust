using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.GridBinding
{
	public class GridRowSection<T>
	{
		private readonly GridRowSection<T> _previousGridRowSection;
		private readonly ColumnEntityBinder<T> _columnEntityBinder;
		private readonly int _sectionStartPosition;
		private readonly IList<GridRow<T>> _gridRowsInSection = new List<GridRow<T>>();

		public event EventHandler<GridRowSectionSelectionChangedEventArgs> GridRowSectionSelectionChanged;

		public GridRowSection(ColumnEntityBinder<T> columnEntityBinder)
		{
			_columnEntityBinder = columnEntityBinder;
			_sectionStartPosition = columnEntityBinder.RowCount();
		}

		public GridRowSection(ColumnEntityBinder<T> columnEntityBinder, GridRowSection<T> previousGridRowSection)
			: this(columnEntityBinder)
		{
			_previousGridRowSection = previousGridRowSection;
		}

		public void InsertRow(GridRow<T> gridRow)
		{
			var startPosition = GetInsertStartPosition() + _gridRowsInSection.Count;

			_columnEntityBinder.InsertRow(startPosition, gridRow);
			_gridRowsInSection.Add(gridRow);

		}

		public void DeleteRowWithTag(object tag)
		{
			var row = _gridRowsInSection.FirstOrDefault(r => r.Tag == tag);
			if (row != null)
			{
				_columnEntityBinder.RemoveRow(row);
				_gridRowsInSection.Remove(row);
			}
		}

		public void ClearRows()
		{
			foreach (var gridRow in _gridRowsInSection)
			{
				_columnEntityBinder.RemoveRow(gridRow);
			}
			_gridRowsInSection.Clear();
		}

		public void UpdateRowHeaderTextWithTag(object tag, string headerText)
		{
			var row = _gridRowsInSection.FirstOrDefault(r => r.Tag == tag);
			row.HeaderText = headerText;
		}

		private int GetInsertStartPosition()
		{
			int startPosition = _sectionStartPosition;
			if (_previousGridRowSection != null)
				startPosition += _previousGridRowSection.RowCount();
			return startPosition;
		}

		public int RowCount()
		{
			return _gridRowsInSection.Count;
		}

		public IEnumerable<GridRow<T>> GetSelectedRows(GridRangeInfoList selection)
		{
			GridRangeInfo sectionRange = GetSectionRange();
			HashSet<GridRow<T>> uniqueRows = new HashSet<GridRow<T>>();
			foreach (GridRangeInfo gridRangeInfo in selection)
			{
				var intersection = sectionRange.IntersectRange(gridRangeInfo);
				if (intersection.IsEmpty) continue;

				var adjustedIntersection = intersection.OffsetRange(-sectionRange.Top, 0);
				for (int i = adjustedIntersection.Top; i <= adjustedIntersection.Bottom; i++)
				{
					uniqueRows.Add(_gridRowsInSection[i]);
				}
			}
			return uniqueRows;
		}

		public void IsInsideSection(GridRangeInfo range)
		{
			if (range.IsEmpty || _gridRowsInSection.Count == 0) return;

			GridRangeInfo sectionRange = GetSectionRange();

			var gridRowSectionSelectionChanged = GridRowSectionSelectionChanged;
			if (gridRowSectionSelectionChanged != null)
			{
				var eventArgs = new GridRowSectionSelectionChangedEventArgs { SectionSelected = sectionRange.Contains(range) };
				gridRowSectionSelectionChanged.Invoke(this, eventArgs);
			}
		}

		private GridRangeInfo GetSectionRange()
		{
			var startPosition = GetGridRangeStartPosition();
			var endPositionWithLastRowInclusive = startPosition + _gridRowsInSection.Count - 1;
			return GridRangeInfo.Rows(startPosition, endPositionWithLastRowInclusive);
		}

		private int GetGridRangeStartPosition()
		{
			return GetInsertStartPosition() + _columnEntityBinder.ContentRowOffset();
		}
	}
}
