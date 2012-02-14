using System.Collections.Generic;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public class SFGridNumericCellColumn<T> : SFGridColumnBase<T>
    {
        private readonly IComparer<T> _columnComparer;
        private readonly string _cellModel;
        private readonly int _preferredWidth;

        public SFGridNumericCellColumn(string bindingProperty, string headerText, IComparer<T> columnComparer, 
            string cellModel, int preferredWidth): base(bindingProperty, headerText)
        {
            _columnComparer = columnComparer;
            _cellModel = cellModel;
            _preferredWidth = preferredWidth;
        }

        public override IComparer<T> ColumnComparer
        {
            get
            {
                return _columnComparer;
            }
        }

        public override int PreferredWidth
        {
            get { return _preferredWidth; }
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellType = _cellModel;
            e.Style.CellValue = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            if (e.Style.CellValue!=null && e.Style.CellValue!=System.DBNull.Value)
                PropertyReflectorHelper.SetValue(currentItem, BindingProperty, e.Style.CellValue);
        }
    }
}