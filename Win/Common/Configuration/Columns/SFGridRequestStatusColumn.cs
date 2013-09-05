using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public class SFGridRequestStatusColumn<T> : SFGridColumnBase<T>
    {
        public SFGridRequestStatusColumn(string bindingProperty, string headerText)
            : base(bindingProperty, headerText)
        { _isReadonly = true; }

        private readonly int _textWidth;
        private readonly bool _isReadonly;

        public SFGridRequestStatusColumn(string bindingProperty, string headerText, int textWidth, bool readOnly)
            : base(bindingProperty, headerText)
        {
            _textWidth = textWidth;
            _isReadonly = readOnly;
        }
        public override int PreferredWidth
        {
            get { return (_textWidth > 0) ? _textWidth : 150; }
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellType = "RequestStatusCell";
            e.Style.CellValue = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
            e.Style.ReadOnly = _isReadonly;
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            return;
        }

    }
}