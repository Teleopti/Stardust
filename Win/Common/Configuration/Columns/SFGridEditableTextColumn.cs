using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public class SFGridEditableTextColumn<T> : SFGridColumnBase<T>
    {
        private readonly int _maxLength;

        public SFGridEditableTextColumn(string bindingProperty, int maxLength, string headerText)
            : base(bindingProperty, headerText)
        {
            _maxLength = maxLength;
        }

        public override int PreferredWidth
        {
            get { return 100; }
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellValue = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
            e.Style.MaxLength = _maxLength;
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            if (AllowEmptyValue)
            {
                if (string.IsNullOrEmpty((string)e.Style.CellValue))
                    e.Style.CellValue = null;
                else if (((string)e.Style.CellValue).Length > _maxLength)
                    return;
            }
            else
            {
                if (string.IsNullOrEmpty((string)e.Style.CellValue))
                    return;
            }

            PropertyReflectorHelper.SetValue(currentItem, BindingProperty, e.Style.CellValue);
        }
    }
}