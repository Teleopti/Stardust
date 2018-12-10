using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns
{
    public class NullableDateOnlyColumn<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private readonly string _headerText;
        private readonly DateTimeFormatInfo _dtfi;
        
 
        public NullableDateOnlyColumn(string bindingProperty, string headerText) : base(bindingProperty,100)
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

                DateOnly? value = _propertyReflector.GetValue(dataItem, BindingProperty) as DateOnly?;
                DateTime? dateTimeValue = null;
                if (value.HasValue) dateTimeValue = value.Value.Date;
                e.Style.CellValue = dateTimeValue;

                OnCellDisplayChanged(dataItem, e);

            }
            e.Handled = true;
        }


        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.ColIndex > 0 && e.RowIndex > 0)
            {
                if (dataItems.Count == 0) return;
                DateTime dt;
                DateOnly? dateOnly = null;

                if (e.Style.CellValue is DateTime)
                {
                    dateOnly = new DateOnly((DateTime) e.Style.CellValue);
                }
                else if (DateTime.TryParse(e.Style.CellValue.ToString(), out dt))
                {
                    dateOnly = new DateOnly(dt);
                }
                T dataItem = dataItems[e.RowIndex - 1];
                _propertyReflector.SetValue(dataItem, BindingProperty, dateOnly);
                OnCellChanged(dataItem, e);
                e.Handled = true;
            }

        }
    }
}
