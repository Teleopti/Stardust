#region Imports

using System;
using System.Collections.ObjectModel;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.Win.PeopleAdmin;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.Common;
using System.Globalization;

#endregion

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{

    /// <summary>
    /// Represents the read only column for long hour minutes or integer.
    /// </summary>
    /// <remarks>
    /// Created by: Savani Nirasha
    /// Created date: 8/25/2008
    /// </remarks>
    public class LongHourMinutesOrIntegerColumn<T> : ColumnBase<T>
    {

        #region Fields - Instance Member

        /// <summary>
        /// Holds the property reflector to read and write object property data.
        /// </summary>
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        /// <summary>
        /// holda the text of the column header.
        /// </summary>
        private string _headerText;

        /// <summary>
        /// Holds the group column header, if it is a group column.
        /// </summary>
        private string _groupHeaderText;

        /// <summary>
        /// Holds the binding property of the column.
        /// </summary>
        private string _bindingProperty;

        /// <summary>
        /// Holds whether the column is read only or not.
        /// </summary>
        private bool _readOnly;

        /// <summary>
        /// Holdst the explicit column disable condition.
        /// </summary>
        private IColumnDisableCondition<T> _columnDisableCondition;

        #endregion

        #region Properties - Instance Member

        #region Properties - Instance Member - ReadOnlyLongHourMinutesOrIntegerColumn Members

        #endregion

        #region Properties - Instance Member - ColumnBase Members - Overriden

        /// <summary>
        /// Gets the width of the preferred.
        /// </summary>
        /// <value>The width of the preferred.</value>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 8/25/2008
        /// </remarks>
        public override int PreferredWidth
        {
            get { return 150; }
        }

        /// <summary>
        /// Gets the binding property.
        /// </summary>
        /// <value>The binding property.</value>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 8/25/2008
        /// </remarks>
        public override string BindingProperty
        {
            get
            {
                return _bindingProperty;
            }
        }

        #endregion

        #endregion

        #region Events - Instance Member

        #region Events - Instance Member - ColumnBase Members - Overrided

        /// <summary>
        /// Gets the cell info.
        /// </summary>
        /// <param name="e">The 
        /// <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> 
        /// instance containing the event data.</param>
        /// <param name="dataItems">The data items.</param>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 8/25/2008
        /// </remarks>
        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            // Sets th ecell headers
            if (string.IsNullOrEmpty(_groupHeaderText))
            {
                SetUpSingleHeader(e, dataItems);
            }
            else
            {
                SetUpMultipleHeaders(e, dataItems);
            }

            // Sets the cell type
            e.Style.ReadOnly = this._readOnly;

            if (_readOnly)
                e.Style.TextColor = Color.DimGray;

            e.Handled = true;
        }

        /// <summary>
        /// Saves the cell info.
        /// </summary>
        /// <param name="e">The
        ///  <see cref="Syncfusion.Windows.Forms.Grid.GridSaveCellInfoEventArgs"/> 
        /// instance containing the event data.</param>
        /// <param name="dataItems">The data items.</param>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 8/25/2008
        /// </remarks>
        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if ((e.Style.CellType != "Test") && (!_readOnly))// If cell is grayed
            {
                Type type;
                var isValidValue = true;
                if (e.Style.CellType == "NumericCell")
                {
                    type = typeof(int);
                    int outInt;
                    if (!int.TryParse(e.Style.CellValue.ToString(), out outInt))
                        isValidValue = false;
                }
                else
                {
                    type = typeof(TimeSpan);
                    TimeSpan outTimeSpan;
                    if (!TimeSpan.TryParse(e.Style.CellValue.ToString(), out outTimeSpan))
                        isValidValue = false;
                }

                if (string.IsNullOrEmpty(_groupHeaderText))
                {
                    if (isValidValue)
                    {
                        if (e.ColIndex > 0 && e.RowIndex > 0)
                        {
                            T dataItem = dataItems[e.RowIndex - 1];
                            _propertyReflector.SetValue(dataItem, _bindingProperty,
                                                        Convert.ChangeType(e.Style.CellValue, type,
                                                                           CultureInfo.InvariantCulture));
                            OnCellChanged(dataItem, e);
                        }
                    }
                    
                }
                else
                {
                    if (e.ColIndex > 0 && e.RowIndex > 1)
                    {
                        T dataItem = dataItems[e.RowIndex - 2];
                        _propertyReflector.SetValue(dataItem, _bindingProperty,
                                                    Convert.ChangeType(e.Style.CellValue, type,
                                                                       CultureInfo.InvariantCulture));
                        OnCellChanged(dataItem, e);
                    }
                }
            }

            e.Handled = true;
        }

        #endregion

        #endregion

        #region Methods - Instance Member

        #region Methods - Instance Member - ReadOnlyLongHourMinutesOrIntegerColumn Members

        #region Methods - Instance Member - ReadOnlyLongHourMinutesOrIntegerColumn Members - Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LongHourMinutesOrIntegerColumn&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="bindingProperty">The binding property.</param>
        /// <param name="headerText">The header text.</param>
        /// <param name="readOnly">if set to <c>true</c> [read only].</param>
        /// <param name="columnDisableCondition">The column disable condition.</param>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 8/25/2008
        /// </remarks>
        public LongHourMinutesOrIntegerColumn(string bindingProperty, string headerText, bool readOnly, 
            IColumnDisableCondition<T> columnDisableCondition)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
            _readOnly = readOnly;
            _columnDisableCondition = columnDisableCondition;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LongHourMinutesOrIntegerColumn&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="bindingProperty">The binding property.</param>
        /// <param name="headerText">The header text.</param>
        /// <param name="groupHeaderText">The group header text.</param>
        /// <param name="readOnly">if set to <c>true</c> [read only].</param>
        /// <param name="columnDisableCondition">The column disable condition.</param>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 8/25/2008
        /// </remarks>
        public LongHourMinutesOrIntegerColumn(string bindingProperty, string headerText,
            string groupHeaderText, bool readOnly, IColumnDisableCondition<T> columnDisableCondition)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
            _groupHeaderText = groupHeaderText;
            _readOnly = readOnly;
            _columnDisableCondition = columnDisableCondition;
        }

        #endregion

        #endregion

        #region Setup Headers

        /// <summary>
        /// Set up single header.
        /// </summary>
        /// <param name="e">The 
        /// <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> 
        /// instance containing the event data.</param>
        /// <param name="dataItems">The data items.</param>
        /// <remarks>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 8/25/2008
        /// </remarks>
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
                object obj = _propertyReflector.GetValue(dataItem, _bindingProperty);

                if (obj != null)
                {
                    // Sets the cell model
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

        /// <summary>
        /// Sets up multiple headers.
        /// </summary>
        /// <param name="e">The 
        /// <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> 
        /// instance containing the event data.</param>
        /// <param name="dataItems">The data items.</param>
        /// <remarks>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 8/25/2008
        /// </remarks>
        private void SetUpMultipleHeaders(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex == 0)
            {
                e.Style.CellValue = _groupHeaderText;
                e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;
            }
            else if (e.RowIndex == 1 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
            }
            else
            {
                T dataItem = dataItems[e.RowIndex - 2];
                object obj = _propertyReflector.GetValue(dataItem, _bindingProperty);

                // Sets the cell model
                if (e.Style.CellValueType == typeof(int))
                {
                    e.Style.CellType = "NumericCell";
                }
                else
                {
                    e.Style.CellType = "TimeSpanLongHourMinutesCell";
                }

                if (obj != null)
                {
                    e.Style.CellValue = obj;

                }

                PeopleAdminHelper.GrayColumn(_propertyReflector, dataItem, e);
                e.Style.TextColor = Color.DimGray;

                // Sets the column enable functionlity
                SetColumnFunctionality(e, dataItem);
            }
        }

        /// <summary>
        /// Sets the column functionality.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> instance containing the event data.</param>
        /// <param name="dataItem">The data item.</param>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-11-20
        /// </remarks>
        private void SetColumnFunctionality(GridQueryCellInfoEventArgs e, T dataItem)
        {
            if ((!_readOnly) && (e.Style.CellType != "Test"))
            {
                if ((_columnDisableCondition != null) &&
                    (_columnDisableCondition.IsColumnDisable(dataItem, _bindingProperty)))
                {
                    e.Style.ReadOnly = true;
                    e.Style.TextColor = Color.DimGray;
                }
            }
        }

        #endregion

        #endregion

    }

    /// <summary>
    /// Represents the interface for the column disable condition.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: Savani Nirasha
    /// Created date: 2008-11-20
    /// </remarks>
    public interface IColumnDisableCondition<T>
    {
        bool IsColumnDisable(T dataItem, string bindingProperty);
    }

}
