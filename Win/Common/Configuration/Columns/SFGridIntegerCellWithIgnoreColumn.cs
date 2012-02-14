using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public class SFGridIntegerCellWithIgnoreColumn<T> : SFGridColumnBase<T>
    {
        private readonly IComparer<T> _columnComparer;

        public SFGridIntegerCellWithIgnoreColumn(string bindingProperty, string headerText, IComparer<T> columnComparer)
            : this(bindingProperty, headerText,null,columnComparer)
        {
        }

        public SFGridIntegerCellWithIgnoreColumn(string bindingProperty, string headerText, string groupHeaderText, IComparer<T> columnComparer)
            : base(bindingProperty, headerText)
        {
            GroupHeaderText = groupHeaderText;
            _columnComparer = columnComparer;
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
            get { return 50; }
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            var value = PropertyReflectorHelper.GetValue(currentItem, BindingProperty) as Int32?;
            if (value.HasValue)
            {
                e.Style.CellType = "IntegerCellModel";
                e.Style.CellValueType = typeof(Int32); 
                e.Style.CellValue = value.Value;
                e.Style.Enabled = true;
            }
            else
            {
                e.Style.CellType = "IgnoreCell";
                e.Style.Enabled = false;
                e.Style.CellValue = new Ignore();
            }
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            if (e.Style.CellValue is Ignore)
                return;

            var cellValue = e.Style.CellValue as string;
            if (cellValue != null && cellValue.Length == 0)
                PropertyReflectorHelper.SetValue(currentItem, BindingProperty, null);
            else
                PropertyReflectorHelper.SetValue(currentItem, BindingProperty, e.Style.CellValue);
        }
    }
}
