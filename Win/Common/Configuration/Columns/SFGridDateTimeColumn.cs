using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public class SFGridDateTimeColumn<T> : SFGridColumnBase<T>
    {
        private readonly int _prefferedWidth;

        public SFGridDateTimeColumn(string bindingProperty, string headerText , int preferredWidth)
            :base(bindingProperty,headerText)
        {
            _prefferedWidth = preferredWidth;
        }

        public override int PreferredWidth
        {
            get { return _prefferedWidth; }
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            object value = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
            
            e.Style.CellType = "DateTimeCellModel";
            e.Style.CellValueType = typeof(DateTime);

            DateTimeFormatInfo dfi = CultureInfo.CurrentCulture.DateTimeFormat;

            string format = string.Format(CultureInfo.CurrentCulture, "{0} {1}", dfi.ShortDatePattern, dfi.ShortTimePattern);
            e.Style.Format = format;
            
            
            if (value is DateTime)
            {
            	e.Style.Enabled = true;
            	e.Style.CellValue = ((DateTime)value==DateTime.MinValue) ? DateTime.Today : value;
            }
            else
            {
                e.Style.CellType = "IgnoreCell";
                e.Style.Enabled = false;
            }
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            if (e.Style.CellValue != DBNull.Value && e.Style.CellValue != null)
            {
                DateTime newDateTimeValue;
                if (DateTime.TryParse(e.Style.CellValue.ToString(), out newDateTimeValue))
                {
                    PropertyReflectorHelper.SetValue(currentItem, BindingProperty, newDateTimeValue);
                    return;
                }
            }
            PropertyReflectorHelper.SetValue(currentItem, BindingProperty, null);
        }

    }
}
