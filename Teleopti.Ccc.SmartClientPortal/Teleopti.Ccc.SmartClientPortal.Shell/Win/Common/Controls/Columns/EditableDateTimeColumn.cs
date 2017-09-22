using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns
{
    public class EditableDateTimeColumn<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private string _headerText;
        private DateTimeFormatInfo _dtfi;

        public EditableDateTimeColumn(string bindingProperty, string headerText)
            : base(bindingProperty, 100)
        {
            _headerText = headerText;

            _dtfi = CultureInfo.CurrentCulture.DateTimeFormat;
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
            }
            else
            {
                if (dataItems.Count == 0) return;
				e.Style.CellType = GridCellModelConstants.CellTypeDatePickerCell;
                e.Style.Format = _dtfi.ShortDatePattern;
                e.Style.CellValueType = typeof(DateTime);
                T dataItem = dataItems[e.RowIndex - 1];
                e.Style.CellValue = _propertyReflector.GetValue(dataItem, BindingProperty);
                OnCellDisplayChanged(dataItem, e);
            }
            e.Handled = true;
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.ColIndex > 0 && e.RowIndex > 0)
            {
                if (dataItems.Count == 0) return;
                DateTime? dt = null;
                DateTime dateFromParse;
                if (e.Style.CellValue is DateTime)
                {
                    dt = (DateTime?) e.Style.CellValue;
                } else if (DateTime.TryParse(e.Style.CellValue.ToString(), out dateFromParse))
                {
                    dt = dateFromParse;
                }

                if (!dt.HasValue)
                {
                    dt = DateTime.MinValue;
                }

                T dataItem = dataItems[e.RowIndex - 1];
                _propertyReflector.SetValue(dataItem, BindingProperty, dt);
                OnCellChanged(dataItem, e);
                e.Handled = true;
            }
        }
    }
}