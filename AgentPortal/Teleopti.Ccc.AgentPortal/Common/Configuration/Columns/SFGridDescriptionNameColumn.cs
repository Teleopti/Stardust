using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.AgentPortal.Common.Configuration.Columns
{
    public class SFGridDescriptionNameColumn<T> : SFGridColumnBase<T>
    {
        public SFGridDescriptionNameColumn(string bindingProperty, string headerText, int preferredWidth)
            : base(bindingProperty, headerText, preferredWidth)
        { }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellType = "DescriptionNameCell";
            e.Style.CellValue = GetDescription(currentItem);
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            Description description = new Description((string)e.Style.CellValue);
            PropertyReflectorHelper.SetValue(currentItem, BindingProperty, new Description(description.Name, description.ShortName));
        }

        private Description GetDescription(T currentItem)
        {
            Description description;
            object value = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
            if (value == null) return new Description();

            if (value is Description)
                description = (Description)value;
            else
                description = new Description(value.ToString(), System.String.Empty);
            return description;
        }
    }
}