using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Controls.Columns
{
    public class EditableDateOnlyColumnForPeriodGrids<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private readonly string _headerText;
        private readonly string _bindingProperty;
        private readonly DateTimeFormatInfo _dtfi;
        
        public EditableDateOnlyColumnForPeriodGrids(string bindingProperty, string headerText) : base(bindingProperty,100)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;

            _dtfi = CultureInfo.CurrentCulture.DateTimeFormat;
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
            }
            
            if (IsContentRow(e.RowIndex,dataItems.Count))
            {
				e.Style.CellType = GridCellModelConstants.CellTypeDatePickerCell;
                e.Style.Format = _dtfi.ShortDatePattern;
                e.Style.CellValueType = typeof(DateTime);
                T dataItem = dataItems[e.RowIndex - 1];

                DateOnly? value = _propertyReflector.GetValue(dataItem, _bindingProperty) as DateOnly?;
                DateTime? dateTimeValue = null;
                if (value.HasValue) dateTimeValue = value.Value.Date;
                e.Style.CellValue = dateTimeValue;

                PeopleAdminHelper.GrayColumn(_propertyReflector, dataItem, e);
                OnCellDisplayChanged(dataItem, e);
            }
            e.Handled = true;
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.ColIndex > 0 && e.RowIndex > 0)
            {
                if (dataItems.Count == 0) return;
                DateTime dt;
                DateOnly? date = null;

                if (e.Style.CellValue == null) return;
                if (e.Style.CellType == "Test") 
                    return;

                if (e.Style.CellValue is DateTime)
                {
                    date = new DateOnly(((DateTime)e.Style.CellValue).LimitMin());
                } 
                else if (DateTime.TryParse(e.Style.CellValue.ToString(), out dt))
                {
                    date = new DateOnly(DateTime.SpecifyKind(dt, DateTimeKind.Unspecified).LimitMin());
                }
                if (date.HasValue)
                {
                    T dataItem = dataItems[e.RowIndex - 1];
                    _propertyReflector.SetValue(dataItem, _bindingProperty, date.Value);
                    OnCellChanged(dataItem, e);
                }
                e.Handled = true;
            }
        }
    }
}

