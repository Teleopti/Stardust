using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public class SFGridWrappedTextReadOnlyColumn<T> : SFGridColumnBase<T>
    {
        private const int ColumnDefaultWidth = 100;
        public SFGridWrappedTextReadOnlyColumn(string bindingProperty, string headerText)
            : base(bindingProperty, headerText)
        { 
            _isReadonly = true;
            _textWidth = ColumnDefaultWidth;
        }

        private readonly int _textWidth;
        private readonly bool _isReadonly;

        public SFGridWrappedTextReadOnlyColumn(string bindingProperty, string headerText, int textWidth, bool isReadOnly)
            : base(bindingProperty, headerText)
        {
            _textWidth = textWidth;
            _isReadonly = isReadOnly;
        }
        public override int PreferredWidth
        {
            get { return (_textWidth > 0 && _textWidth < 300) ? _textWidth : 300; }
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellType = "WrappedTextReadOnlyCell";
            e.Style.CellValue = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
            e.Style.ReadOnly = _isReadonly;
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            return;
        }
    }
}