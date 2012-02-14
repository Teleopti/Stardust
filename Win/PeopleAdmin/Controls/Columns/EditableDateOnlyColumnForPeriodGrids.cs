using System;
using System.Globalization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.Common;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.Win.PeopleAdmin.Controls.Columns
{
    public class EditableDateOnlyColumnForPeriodGrids<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private readonly string _headerText;
        private readonly string _bindingProperty;
        private readonly DateTimeFormatInfo _dtfi;
        private readonly bool CheckIfCanGray = true;

        public EditableDateOnlyColumnForPeriodGrids(string bindingProperty, string headerText, bool checkIfCanGray)
            : this(bindingProperty, headerText)
        {
            CheckIfCanGray = checkIfCanGray;
        }

        public EditableDateOnlyColumnForPeriodGrids(string bindingProperty, string headerText)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;

            CultureInfo ci = new CultureInfo(CultureInfo.CurrentCulture.LCID, false);
            _dtfi = ci.DateTimeFormat;
        }

        public override int PreferredWidth
        {
            get { return 100; }
        }

        public override string BindingProperty
        {
            get
            {
                return _bindingProperty;
            }
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
            }
            
            if (IsContentRow(e.RowIndex,dataItems.Count))
            {
                e.Style.CellType = "DatePickerCell";
                e.Style.Format = _dtfi.ShortDatePattern;
                e.Style.CellValueType = typeof(DateTime);
                T dataItem = dataItems[e.RowIndex - 1];

                DateOnly? value = _propertyReflector.GetValue(dataItem, _bindingProperty) as DateOnly?;
                DateTime? dateTimeValue = null;
                if (value.HasValue) dateTimeValue = value.Value;
                e.Style.CellValue = dateTimeValue;
                
                if (CheckIfCanGray)
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

                if (e.Style.CellValue == null) return;
                if (e.Style.CellType == "Test") 
                    return;

                if (DateTime.TryParse(e.Style.CellValue.ToString(), out dt))
                {
                    T dataItem = dataItems[e.RowIndex - 1];
                    _propertyReflector.SetValue(dataItem, _bindingProperty, new DateOnly(DateTime.SpecifyKind(dt, DateTimeKind.Unspecified).LimitMin()));
                    OnCellChanged(dataItem, e);
                }
                e.Handled = true;
            }
        }
    }
}

