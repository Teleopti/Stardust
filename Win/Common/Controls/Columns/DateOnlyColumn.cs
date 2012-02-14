using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Win.Common.Configuration.Columns;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
    public class DateOnlyColumn<T> : SFGridColumnBase<T>
    {
        private readonly DateTimeFormatInfo _dtfi;
        
        public DateOnlyColumn(string bindingProperty, string headerText) : this(bindingProperty,headerText,null)
        {
        }

        public DateOnlyColumn(string bindingProperty, string headerText, string groupHeaderText)
            : base(bindingProperty, headerText)
        {
            GroupHeaderText = groupHeaderText;
            CultureInfo ci = new CultureInfo(CultureInfo.CurrentCulture.LCID, false);
            _dtfi = ci.DateTimeFormat;
        }

        public override int PreferredWidth
        {
            get { return 100; }
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            if (dataItems.Count == 0) return;

            DateOnly? value = PropertyReflectorHelper.GetValue(currentItem, BindingProperty) as DateOnly?;
            if (value.HasValue)
            {
                e.Style.CellType = "DatePickerCell";
                e.Style.Format = _dtfi.ShortDatePattern;
                e.Style.CellValueType = typeof(DateTime);
                e.Style.CellValue = value.Value.Date;
                e.Style.Enabled = true;
            }
            else
            {
                e.Style.CellType = "IgnoreCell";
                e.Style.Enabled = false;
                e.Style.CellValue = new Ignore();
            }
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            if (dataItems.Count == 0) return;
            DateTime dt;
            DateOnly? dateOnly = null;

            if (DateTime.TryParse(e.Style.CellValue.ToString(), out dt))
            {
                dateOnly = new DateOnly(dt);
            }
            PropertyReflectorHelper.SetValue(currentItem, BindingProperty, dateOnly);
            e.Handled = true;
        }
    }
}
