using System;
using System.Collections.ObjectModel;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Controls.Columns
{
    public class ReadOnlyHourMinutesColumnForSchedulePeriod<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private readonly string _headerText;
        
        public ReadOnlyHourMinutesColumnForSchedulePeriod(string bindingProperty, string headerText) : base(bindingProperty, 150)
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
            if (IsContentRow(e.RowIndex,dataItems.Count))
            {
				e.Style.CellType = GridCellModelConstants.CellTypeTimeSpanLongHourMinutesOrEmptyCell;
                e.Style.ReadOnly = true;
                T dataItem = dataItems[e.RowIndex - 1];

                object obj = _propertyReflector.GetValue(dataItem, BindingProperty);
                TimeSpan timeSpan = (TimeSpan) obj;

                if (timeSpan != TimeSpan.MinValue)
                {
                    e.Style.CellValue = obj;
                }
                
                PeopleAdminHelper.GrayColumn(_propertyReflector, dataItem, e);
                OnCellDisplayChanged(dataItem, e);
                e.Style.TextColor = Color.DimGray;
            }
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            e.Handled = true;
        }
    }
}