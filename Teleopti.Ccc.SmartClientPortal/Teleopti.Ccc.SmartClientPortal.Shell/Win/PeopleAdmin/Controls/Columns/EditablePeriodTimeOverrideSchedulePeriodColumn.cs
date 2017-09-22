using System;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Controls.Columns
{
	public class EditablePeriodTimeOverrideSchedulePeriodColumn<T> : ColumnBase<T>
	{
		private readonly PropertyReflector _propertyReflector = new PropertyReflector();
		private readonly string _headerText;

		public EditablePeriodTimeOverrideSchedulePeriodColumn(string bindingProperty, string headerText) : base(bindingProperty,150)
		{
			_headerText = headerText;
		}

		public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
		{
			SetUpSingleHeader(e, dataItems);
			e.Handled = true;
		}

		private void SetUpSingleHeader(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
		{
			if (e.RowIndex == 0 && e.ColIndex > 0)
			{
				e.Style.CellValue = _headerText;
				e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;
			}

			if (IsContentRow(e.RowIndex, dataItems.Count))
			{
				e.Style.CellType = GridCellModelConstants.CellTypeTimeSpanLongHourMinutesOrEmptyCell;
				T dataItem = dataItems[e.RowIndex - 1];

				object obj = _propertyReflector.GetValue(dataItem, BindingProperty);
				e.Style.CellValue = obj;

				PeopleAdminHelper.GrayColumn(_propertyReflector, dataItem, e);
				OnCellDisplayChanged(dataItem, e);
			}
		}

		public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
		{
			if (e.ColIndex <= 0 || e.RowIndex <= 0) return;
			
			var dataItem = dataItems[e.RowIndex - 1];
			
			if (e.Style.CellValue == null || (TimeSpan)e.Style.CellValue == TimeSpan.Zero)
				PeopleAdminHelper.ResetPeriodTime(dataItem);
			else
				_propertyReflector.SetValue(dataItem, BindingProperty, e.Style.CellValue);

			OnCellChanged(dataItem, e);
			e.Handled = true;
		}
	}
}