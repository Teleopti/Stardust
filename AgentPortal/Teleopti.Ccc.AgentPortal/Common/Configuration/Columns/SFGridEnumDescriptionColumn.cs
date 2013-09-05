using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortalCode.Common;

namespace Teleopti.Ccc.AgentPortal.Common.Configuration.Columns
{
    public class SFGridEnumDescriptionColumn<T> : SFGridColumnBase<T>
    {
        public SFGridEnumDescriptionColumn(string bindingProperty, string headerText)
            : base(bindingProperty, headerText)
        { }

        public SFGridEnumDescriptionColumn(string bindingProperty, string headerText, int preferredWidth)
            : base(bindingProperty, headerText, preferredWidth)
        { }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellType = "Static";
            e.Style.CellValue = GetDescription(currentItem);
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
        }

        private object GetDescription(T currentItem)
        {
            object value = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
            return LanguageResourceHelper.TranslateEnumValue(value);
        }
    }
}