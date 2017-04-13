using System;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public class SFGridHourMinutesColumn<T> : SFGridColumnBase<T>
    {
        public SFGridHourMinutesColumn(string bindingProperty, string headerText) : base(bindingProperty, headerText)
        {
        }

        public override int PreferredWidth
        {
            get { return 100; }
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            object value = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);

            if (value != null)
            {
                e.Style.CellType = "HourMinutes";
                e.Style.Enabled = true;
                e.Style.CellValue = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
            }
            else
            {
                e.Style.CellValue = new Ignore();
                e.Style.Enabled = false;
                e.Style.CellType = "IgnoreCell";
            }
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            if (e.Style.CellValue != null && !(e.Style.CellValue is Ignore))
            {
                TimeSpan t;
                if (TimeSpan.TryParse(e.Style.CellValue.ToString(), out t))
                {
                    PropertyReflectorHelper.SetValue(currentItem, BindingProperty, t);
                    return;
                }
            }
            PropertyReflectorHelper.SetValue(currentItem, BindingProperty, null);
        }
    }
}