using System;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns
{
    class SFGridNullableIntegerCellColumn<T> : SFGridColumnBase<T>
    {
    	private readonly int _preferredWidth = 50;
       
		public SFGridNullableIntegerCellColumn(string bindingProperty, string headerText, int preferredWidth)
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
            e.Style.CellType = "NullableIntegerCellModel";
            e.Style.CellValueType = typeof (Int32);
            e.Style.CellValue = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            if (e.Style.CellValue != null && string.IsNullOrEmpty(e.Style.CellValue.ToString()))
                PropertyReflectorHelper.SetValue(currentItem, BindingProperty, null);
            else
            PropertyReflectorHelper.SetValue(currentItem, BindingProperty, e.Style.CellValue);
        }
    }
}
