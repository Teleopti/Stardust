using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns
{
    public class LongHourMinutesOrIntegerColumn<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private string _headerText;
        private bool _readOnly;

        private IColumnDisableCondition<T> _columnDisableCondition;

        public LongHourMinutesOrIntegerColumn(string bindingProperty, string headerText, bool readOnly,
            IColumnDisableCondition<T> columnDisableCondition)
            : base(bindingProperty, 150)
        {
            _headerText = headerText;
            _readOnly = readOnly;
            _columnDisableCondition = columnDisableCondition;
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            SetUpSingleHeader(e, dataItems);
         
            e.Style.ReadOnly = _readOnly;

            if (_readOnly)
                e.Style.TextColor = Color.DimGray;

            e.Handled = true;
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if ((e.Style.CellType != "Test") && (!_readOnly)) // If cell is grayed
            {
                Type type;
                var isValidValue = true;
                if (e.Style.CellType == "NumericCell")
                {
                    type = typeof (int);
					if (!int.TryParse(e.Style.CellValue.ToString(), out _))
                        isValidValue = false;
                }
                else
                {
                    type = typeof (TimeSpan);
					if (!TimeSpan.TryParse(e.Style.CellValue.ToString(), out _))
                        isValidValue = false;
                }

                if (isValidValue)
                {
                    if (e.ColIndex > 0 && e.RowIndex > 0)
                    {
                        T dataItem = dataItems[e.RowIndex - 1];
                        _propertyReflector.SetValue(dataItem, BindingProperty,
                                                    Convert.ChangeType(e.Style.CellValue, type,
                                                                       CultureInfo.InvariantCulture));
                        OnCellChanged(dataItem, e);
                    }
                }
            }

            e.Handled = true;
        }

        private void SetUpSingleHeader(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
                e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;
            }
            else
            {
                T dataItem = dataItems[e.RowIndex - 1];
                object obj = _propertyReflector.GetValue(dataItem, BindingProperty);

                if (obj != null)
                {
                    if (obj is int)
                    {
                        e.Style.CellType = "NumericCell";
                    }
                    else
                    {
                        e.Style.CellType = "TimeSpanLongHourMinutesCell";
                    }

                    e.Style.CellValue = obj;
                }

                PeopleAdminHelper.GrayColumn(_propertyReflector, dataItem, e);

                OnCellDisplayChanged(dataItem, e);

                if (_readOnly)
                    e.Style.TextColor = Color.DimGray;

                SetColumnFunctionality(e, dataItem);
            }
        }

        private void SetColumnFunctionality(GridQueryCellInfoEventArgs e, T dataItem)
        {
            if ((!_readOnly) && (e.Style.CellType != "Test"))
            {
                if ((_columnDisableCondition != null) &&
                    (_columnDisableCondition.IsColumnDisable(dataItem, BindingProperty)))
                {
                    e.Style.ReadOnly = true;
                    e.Style.TextColor = Color.DimGray;
                }
            }
        }
    }
}
