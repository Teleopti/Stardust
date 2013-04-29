using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public class SFGridEditableDateColumn<T>:  SFGridColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private readonly string _headerText;
        private readonly string _bindingProperty;
        private readonly DateTimeFormatInfo _dtfi;


        public SFGridEditableDateColumn(string bindingProperty, string headerText)
            : base(bindingProperty, headerText)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;

            var ci = new CultureInfo(CultureInfo.CurrentCulture.LCID, false);
            _dtfi = ci.DateTimeFormat;
        }

        public override int PreferredWidth
        {
            get { return 100; }
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
            }
            else
            {
                if (dataItems.Count == 0) return;
				e.Style.CellType = GridCellModelConstants.CellTypeDatePickerCell;
                //e.Style.CellType = "MonthCalendar";
                e.Style.Format = _dtfi.ShortDatePattern;
                e.Style.CellValueType = typeof(DateTime);
                e.Style.CellValue = _propertyReflector.GetValue(currentItem, _bindingProperty);
            }
            e.Handled = true;
        }


        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
        	if (e.ColIndex <= 0 || e.RowIndex <= 0) return;
        	if (dataItems.Count == 0) return;

        	if (string.IsNullOrEmpty(e.Style.CellValue.ToString()))
        		_propertyReflector.SetValue(currentItem, _bindingProperty, null);
                
        	DateTime dt;

        	if (DateTime.TryParse(e.Style.CellValue.ToString(), out dt))
        	{
        		_propertyReflector.SetValue(currentItem, _bindingProperty, dt);
        		OnCellChanged(currentItem);
        	}
        	e.Handled = true;
        }
    }
}
