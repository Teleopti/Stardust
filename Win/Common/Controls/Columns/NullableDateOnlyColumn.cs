using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
    public class NullableDateOnlyColumn<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private readonly string _headerText;
        private readonly string _bindingProperty;
        private readonly DateTimeFormatInfo _dtfi;
        
 
        public NullableDateOnlyColumn(string bindingProperty, string headerText)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;

            CultureInfo ci = new CultureInfo(CultureInfo.CurrentCulture.LCID, false);
            _dtfi = ci.DateTimeFormat;

        }

        public override int PreferredWidth
        {
            get { return 100; }
        }

        public override string BindingProperty
        {
            get
            {
                return _bindingProperty;
            }
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

                e.Style.CellType = "DatePickerCell";
                e.Style.Format = _dtfi.ShortDatePattern;
                e.Style.CellValueType = typeof(DateTime);
                T dataItem = dataItems[e.RowIndex - 1];

                DateOnly? value = _propertyReflector.GetValue(dataItem, _bindingProperty) as DateOnly?;
                DateTime? dateTimeValue = null;
                if (value.HasValue) dateTimeValue = value.Value;
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

                if (DateTime.TryParse(e.Style.CellValue.ToString(), out dt))
                {
                    dateOnly = new DateOnly(dt);
                }
                T dataItem = dataItems[e.RowIndex - 1];
                _propertyReflector.SetValue(dataItem, _bindingProperty, dateOnly);
                OnCellChanged(dataItem, e);
                e.Handled = true;
            }

        }
    }
}
