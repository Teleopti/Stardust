using System;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.AgentPortal.Common.Configuration.Columns
{
    public class SFGridHourMinutesColumn<T> : SFGridColumnBase<T>
    {
        private readonly string _toBeBold = string.Empty;
        
        public SFGridHourMinutesColumn(string bindingProperty, string headerText, int preferredWidth)
            : base(bindingProperty, headerText, preferredWidth)
        {
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellType = "HourMinutes";
            e.Style.CellValue = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
            if (!string.IsNullOrEmpty(_toBeBold))
                e.Style.Font.Bold = (bool)PropertyReflectorHelper.GetValue(currentItem, _toBeBold);
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            if (!(e.Style.CellValue is TimeSpan)) return;
            PropertyReflectorHelper.SetValue(currentItem, BindingProperty, (TimeSpan)e.Style.CellValue);
        }
    }
}