using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns
{

    public class SFGridReadOnlyTextColumn<T> : SFGridColumnBase<T>
    {
        private readonly int _preferredWidth;

        public SFGridReadOnlyTextColumn(string bindingProperty, string headerText)
            : base(bindingProperty, headerText)
        { }

        public SFGridReadOnlyTextColumn(string bindingProperty, int preferredWidth, string headerText)
            : base(bindingProperty, headerText)
        {
            _preferredWidth = preferredWidth;
        }

        public override int PreferredWidth
        {
            get { return (_preferredWidth > 0) ? _preferredWidth : 150; }
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellValue = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
            e.Style.ReadOnly = true;
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            // Commented: To avoid getting exceptions when doing copy past 
            //throw new InvalidOperationException("Attempt to set value of read-only cell");
        }
    }
}