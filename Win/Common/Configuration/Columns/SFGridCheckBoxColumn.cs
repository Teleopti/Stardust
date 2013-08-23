using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public class SFGridCheckBoxColumn<T> : SFGridColumnBase<T>
    {
        private readonly int _preferredWidth;
        public SFGridCheckBoxColumn(string bindingProperty, string headerText) : base(bindingProperty, headerText)
        {
            _preferredWidth = 125;
        }

        public SFGridCheckBoxColumn(string bindingProperty, string headerText , int preferredWidth)
            : base(bindingProperty, headerText)
        {
            _preferredWidth = preferredWidth;
        }

        public override int PreferredWidth
        {
            get { return _preferredWidth; }
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellType = "CheckBox";
            e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;
            e.Style.CellValueType = typeof(bool);
            e.Style.CheckBoxOptions.CheckedValue = bool.TrueString;
            e.Style.CheckBoxOptions.UncheckedValue = bool.FalseString;
            e.Style.CellValue = PropertyReflectorHelper.GetValue(currentItem, BindingProperty).ToString();
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            bool b = bool.Parse(e.Style.CellValue.ToString());
            PropertyReflectorHelper.SetValue(currentItem, BindingProperty, b);
        }
    }
}