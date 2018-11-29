using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.GridBinding
{
	public class ColumnEntityBinder<T>
	{
		private readonly GridControl _gridControl;
		private readonly IList<GridRow<T>> _gridRows = new List<GridRow<T>>();
		private IList<T> _collection = new List<T>();
		private IModelProperty<T> _columnHeaderMember;
		private IModelProperty<T> _columnParentHeaderMember;
		private readonly GridRangeChange _rangeChanged;
		private bool _delayUpdates;
		private readonly HashSet<GridRangeInfo> _rangesToUpdate = new HashSet<GridRangeInfo>();
		public GridColors GridColors { get; set; }

		public ColumnEntityBinder(GridControl gridControl)
		{
			_gridControl = gridControl;
			_rangeChanged = new GridRangeChange(gridControl);
		}

		public GridRangeChange RangeChanged
		{
			get { return _rangeChanged; }
		}

		private void gridControl_QueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		{
			var cellDetails = new CellDetails(_gridControl, e.Style);
			cellDetails.GridColors = GridColors;

			if (cellDetails.IsRowHeader)
			{
				cellDetails.SetCellValue(_gridRows[e.RowIndex - 1 - _gridControl.Rows.HeaderCount].HeaderText);
				cellDetails.SetCellTipText(_gridRows[e.RowIndex - 1 - _gridControl.Rows.HeaderCount].CellTipText);
				SetMergedHeaderStyle();
			}
			T item;
			if (cellDetails.IsColumnParentHeader)
			{
				item = _collection[e.ColIndex - 1];
				cellDetails.SetParentCellValue(_columnParentHeaderMember.GetModelValue(item).ToString());
				SetMergedHeaderStyle();
			}
			if (cellDetails.IsColumnHeader)
			{
				item = _collection[e.ColIndex - 1];
				var dateString = _columnHeaderMember.GetModelValue(item).ToString();
				Weekend(cellDetails, _columnHeaderMember.GetModelValue(item));
				cellDetails.SetCellValue(dateString);
				SetMergedHeaderStyle();
			}
			if (cellDetails.IsContentCell)
			{
				item = _collection[e.ColIndex - 1];
				Weekend(cellDetails, _columnHeaderMember.GetModelValue(item));
				var gridRow = _gridRows[e.RowIndex - 1 - _gridControl.Rows.HeaderCount];
				var modelProperty = gridRow.ValueMember;
				cellDetails.SetCellModelType(gridRow.CellValueType);
				cellDetails.SetCellModel(gridRow.CellModel);
				cellDetails.SetCellValue(modelProperty.GetModelValue(item));
				cellDetails.SetReadOnly(gridRow.ReadOnly);
				cellDetails.SetHorizontalAlignment(gridRow.HorizontalAlignment);
			}
		}

		private static void Weekend(CellDetails cellDetails, object date)
		{
			if (WeekendOrWeekday(date))
				cellDetails.SetWeekendStyle();
		}
		private static bool WeekendOrWeekday(object date)
		{
			var dateSupplier = date as IDateSupplier;
			if (dateSupplier == null) return false;

			return DateHelper.IsWeekend(dateSupplier.Date, CultureInfo.CurrentCulture);
		}

		public void SetColumnHeaderMember(IModelProperty<T> property)
		{
			_columnHeaderMember = property;
		}

		public void SetColumnParentHeaderMember(IModelProperty<T> property)
		{
			_columnParentHeaderMember = property;
		}

		public void SetBinding(IList<T> collection)
		{
			BeforeUpdate();

			_collection = collection;

			AfterUpdate();
		}

		private void SetGridArea()
		{
			_gridControl.RowCount = _gridRows.Count + _gridControl.Rows.HeaderCount;
			_gridControl.ColCount = _collection.Count - _gridControl.Cols.HeaderCount;
		}

		private void WireModels()
		{
			if (typeof(INotifyPropertyChanged).IsAssignableFrom(typeof(T)))
			{
				foreach (INotifyPropertyChanged model in _collection)
				{
					model.PropertyChanged += model_PropertyChanged;
				}
			}
		}

		private void WireGrid()
		{
			_gridControl.QueryCellInfo += gridControl_QueryCellInfo;
			_gridControl.SaveCellInfo += gridControl_SaveCellInfo;
			_gridControl.ClientSizeChanged += gridControl_ClientSizeChanged;
		}

		private void gridControl_ClientSizeChanged(object sender, EventArgs e)
		{
			_gridControl.Model.MergeCells.EvaluateMergeCells(GridRangeInfo.Rows(0, 1));
			_gridControl.Model.MergeCells.EvaluateMergeCells(GridRangeInfo.Cols(0, 1));
		}

		private void ResetGridArea()
		{
			_gridControl.RowCount = 0;
			_gridControl.ColCount = 0;
		}

		private void UnwireModels()
		{
			if (typeof(INotifyPropertyChanged).IsAssignableFrom(typeof(T)))
			{
				foreach (INotifyPropertyChanged model in _collection)
				{
					model.PropertyChanged -= model_PropertyChanged;
				}
			}
		}

		private void UnwireGrid()
		{
			_gridControl.QueryCellInfo -= gridControl_QueryCellInfo;
			_gridControl.SaveCellInfo -= gridControl_SaveCellInfo;
			_gridControl.ClientSizeChanged -= gridControl_ClientSizeChanged;
		}

		private void model_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var modelColumnIndex = _collection.IndexOf((T)sender);

			var gridRows = _gridRows.Where(r => r.ValueMember.PropertyName == e.PropertyName);
			foreach (var gridRow in gridRows)
			{
				int rowIndex = _gridRows.IndexOf(gridRow);
				if (rowIndex >= 0 && modelColumnIndex >= 0)
				{
					var cell = GridRangeInfo.Cell(rowIndex + ContentRowOffset(),
											 modelColumnIndex + ContentColumnOffset());
					if (_delayUpdates)
					{
						_rangesToUpdate.Add(cell);
					}
					else
					{
						InvalidateCell(cell);
					}
				}
			}
		}

		private void InvalidateCell(GridRangeInfo cell)
		{
			_gridControl.InvalidateRange(cell);
		}

		public void BeginDelayUpdates()
		{
			_delayUpdates = true;
		}

		public void EndDelayUpdates()
		{
			_delayUpdates = false;
			foreach (var gridRangeInfo in _rangesToUpdate)
			{
				InvalidateCell(gridRangeInfo);
			}
			_rangesToUpdate.Clear();
		}

		private void gridControl_SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
		{
			var cellDetails = new CellDetails(_gridControl, e.Style);
			if (cellDetails.IsContentCell)
			{
				T item = _collection[e.ColIndex - 1];
				var modelProperty = _gridRows[e.RowIndex - 1 - _gridControl.Rows.HeaderCount].ValueMember;
				var typeName = _gridRows[e.RowIndex - 1 - _gridControl.Rows.HeaderCount].CellValueType.Name;
				if (e.Style.IsEmpty)
					e.Style.CellValue = Type.GetType(typeName);
				modelProperty.SetModelValue(item, e.Style.CellValue);
			}
			e.Handled = true;
		}

		public void AddRow(GridRow<T> gridRow)
		{
			_gridRows.Add(gridRow);
		}

		private void AfterUpdate()
		{
			WireGrid();
			WireModels();
			SetGridArea();
		}

		private void BeforeUpdate()
		{
			UnwireGrid();
			UnwireModels();
			ResetGridArea();
		}

		private void SetMergedHeaderStyle()
		{
			_gridControl.Model.Options.MergeCellsMode = GridMergeCellsMode.OnDemandCalculation |
																						 GridMergeCellsMode.MergeColumnsInRow;

			var rowHeaderStyle = _gridControl.Model.BaseStylesMap["Header"].StyleInfo;
			rowHeaderStyle.MergeCell = GridMergeCellDirection.ColumnsInRow;
		}

		public int RowCount()
		{
			return _gridRows.Count;
		}

		public int ContentRowOffset()
		{
			return _gridControl.Rows.HeaderCount + 1;
		}

		public void InsertRow(int position, GridRow<T> gridRow)
		{
			BeforeUpdate();

			_gridRows.Insert(position, gridRow);
			AfterUpdate();
		}

		public void RemoveRow(GridRow<T> gridRow)
		{
			BeforeUpdate();

			_gridRows.Remove(gridRow);

			AfterUpdate();
		}

		public GridControl Grid()
		{
			return _gridControl;
		}

		public int ContentColumnOffset()
		{
			return _gridControl.Cols.HeaderCount + 1;
		}

		public CurrentSelection<T> CurrentSelection()
		{
			return new CurrentSelection<T>(this, _collection);
		}
	}
}
