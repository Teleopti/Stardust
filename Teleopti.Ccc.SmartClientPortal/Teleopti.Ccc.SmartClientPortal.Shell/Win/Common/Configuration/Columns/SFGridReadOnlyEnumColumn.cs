using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns
{
    public class SFGridReadOnlyEnumColumn<T> : SFGridColumnBase<T>
    {
        private const int _preferredWidth = 150;

        public SFGridReadOnlyEnumColumn(string bindingProperty, string headerText)
            : base(bindingProperty, headerText)
        { }

        public override int PreferredWidth
        {
            get { return _preferredWidth; }
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellValue = LanguageResourceHelper.TranslateEnumValue(PropertyReflectorHelper.GetValue(currentItem, BindingProperty));
            e.Style.ReadOnly = true;
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
        }
    }
}