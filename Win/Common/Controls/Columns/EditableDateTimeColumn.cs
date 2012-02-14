using System;
using System.Globalization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.WinCode.Common;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
    public class EditableDateTimeColumn<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private string _headerText;
        private string _bindingProperty;
        private bool _allowNull;
        private DateTimeFormatInfo _dtfi;

        public EditableDateTimeColumn(string bindingProperty, string headerText)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;

            CultureInfo ci = new CultureInfo(CultureInfo.CurrentCulture.LCID, false);
            _dtfi = ci.DateTimeFormat;
        }

        public EditableDateTimeColumn(string bindingProperty, string headerText, bool allowNull)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
            _allowNull = allowNull;

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
            else
            {
                if (dataItems.Count == 0) return;
                e.Style.CellType = "DatePickerCell";
                e.Style.Format = _dtfi.ShortDatePattern;
                e.Style.CellValueType = typeof(DateTime);
                T dataItem = dataItems[e.RowIndex - 1];
                e.Style.CellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);
                OnCellDisplayChanged(dataItem, e);
            }
            e.Handled = true;
        }


        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.ColIndex > 0 && e.RowIndex > 0)
            {
                if (dataItems.Count == 0) return;
                DateTime? dt;
                if (_allowNull)
                {
                    if ((e.Style.CellValue == null) || string.IsNullOrEmpty(e.Style.CellValue.ToString())) dt = null;
                    else try { dt = (DateTime)e.Style.CellValue; }
                        catch (InvalidCastException) { return; }
                }
                else
                    try { dt = (DateTime)e.Style.CellValue; }
                    catch (InvalidCastException) { return; }

                T dataItem = dataItems[e.RowIndex - 1];
                _propertyReflector.SetValue(dataItem, _bindingProperty, dt);
                OnCellChanged(dataItem, e);
                e.Handled = true;
            }

        }
    }
}