using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.AgentPortal.Common.Configuration.Columns
{
    public class SFGridStringColumn<T> : SFGridColumnBase<T>
    {
        public SFGridStringColumn(string bindingProperty, string headerText, int preferredWidth)
            : base(bindingProperty, headerText, preferredWidth)
        { }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellType = "DescriptionNameCell";
            e.Style.CellValue = GetDescription(currentItem);
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
			PropertyReflectorHelper.SetValue(currentItem, BindingProperty, e.Style.CellValue);
        }

        private string GetDescription(T currentItem)
        {
            object value = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
            return (string)value ?? string.Empty;
        }
    }
}