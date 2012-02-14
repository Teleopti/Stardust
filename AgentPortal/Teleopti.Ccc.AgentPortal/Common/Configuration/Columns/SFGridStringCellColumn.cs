using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.AgentPortal.Common.Configuration.Columns
{
    public class SFGridStringCellColumn<T> : SFGridColumnBase<T>
    {
        public SFGridStringCellColumn(string bindingProperty, string headerText)
            : base(bindingProperty, headerText)
        { }

        public SFGridStringCellColumn(string bindingProperty, string headerText, int preferredWidth)
            : base(bindingProperty, headerText, preferredWidth)
        { }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            //e.Style.CellType = "DescriptionNameCell";
            e.Style.CellValue = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            Description description = new Description((string)e.Style.CellValue);
            PropertyReflectorHelper.SetValue(currentItem, BindingProperty, new Description(description.Name, description.ShortName));
        }
    }
}
