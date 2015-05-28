using System;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.GridBinding
{
	public class CellDetails
	{
		private readonly GridControl _gridControl;
		private readonly GridStyleInfo _gridStyleInfo;
		public GridColors GridColors { get; set; }

		public CellDetails(GridControl gridControl, GridStyleInfo gridStyleInfo)
		{
			_gridControl = gridControl;
			_gridStyleInfo = gridStyleInfo;
		}

		public bool IsColumnHeader
		{
			get
			{
				return _gridStyleInfo.CellIdentity != null &&
						_gridStyleInfo.CellIdentity.RowIndex == 1 &&
						_gridStyleInfo.CellIdentity.ColIndex > 0;
			}
		}

		public bool IsColumnParentHeader
		{
			get
			{
				return _gridStyleInfo.CellIdentity != null &&
						_gridStyleInfo.CellIdentity.RowIndex == 0 &&
						_gridStyleInfo.CellIdentity.ColIndex > 0;
			}
		}

		public bool IsRowHeader
		{
			get
			{
				return _gridStyleInfo.CellIdentity != null &&
						_gridControl.Cols.HeaderCount + 1 > _gridStyleInfo.CellIdentity.ColIndex &&
						_gridStyleInfo.CellIdentity.RowIndex > _gridControl.Rows.HeaderCount;
			}
		}

		public bool IsContentCell
		{
			get
			{
				return _gridStyleInfo.CellIdentity != null &&
						_gridControl.Rows.HeaderCount < _gridStyleInfo.CellIdentity.RowIndex &&
						_gridControl.Cols.HeaderCount < _gridStyleInfo.CellIdentity.ColIndex;
			}
		}

		public void SetCellValue(object value)
		{
			_gridStyleInfo.CellValue = value;
		}

		public void SetCellTipText(string value)
		{
			_gridStyleInfo.CellTipText = value;
		}

		public void SetParentCellValue(object value)
		{
			_gridStyleInfo.CellValue = value;
		}

		public void SetCellModelType(Type cellModel)
		{
			_gridStyleInfo.CellValueType = cellModel;
		}

		public void SetCellModel(string cellModel)
		{
			if (string.IsNullOrEmpty(cellModel)) return;
			_gridStyleInfo.CellType = cellModel;
		}

		public void SetReadOnly(bool readOnly)
		{
			_gridStyleInfo.ReadOnly = readOnly;
			if (readOnly)
			{
				if (GridColors.ReadOnlyBackgroundBrush != null)
					_gridStyleInfo.Interior = GridColors.ReadOnlyBackgroundBrush;
				_gridStyleInfo.TextColor = GridColors.ColorReadOnlyCell;
			}
		}

		public void SetWeekendStyle()
		{
			_gridStyleInfo.BackColor = GridColors.ColorHolidayCell;
			if (_gridStyleInfo.CellIdentity.RowIndex == 1)
				_gridStyleInfo.TextColor = GridColors.ColorHolidayHeader;
		}

		public void SetHorizontalAlignment(GridHorizontalAlignment alignement)
		{
			_gridStyleInfo.HorizontalAlignment = alignement;
		}
	}
}