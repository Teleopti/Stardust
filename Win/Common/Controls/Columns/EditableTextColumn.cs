using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.WinCode.Common;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
    public class EditableTextColumn<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private int _maxLength;
        private string _headerText;
        private string _bindingProperty;
        private string _groupHeaderText;


        public EditableTextColumn(string bindingProperty, int maxLength, string headerText)
        {
            _maxLength = maxLength;
            _headerText = headerText;
            _bindingProperty = bindingProperty;
        }

        public EditableTextColumn(string bindingProperty, int maxLength, string headerText, string groupHeaderText)
        {
            _maxLength = maxLength;
            _headerText = headerText;
            _bindingProperty = bindingProperty;
            _groupHeaderText = groupHeaderText;
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
            if (string.IsNullOrEmpty(_groupHeaderText))
            {
                SetUpSingleHeader(e, dataItems);
            }
            else
            {
                SetUpMultipleHeaders(e, dataItems);
            }

            e.Handled = true;
        }

        #region Set Up Headers

        /// <summary>
        /// Set up single header.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> instance containing the event data.</param>
        /// <param name="dataItems">The data items.</param>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-21
        /// </remarks>
        private void SetUpSingleHeader(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
            }
            else
            {
                T dataItem = dataItems[e.RowIndex - 1];
                e.Style.CellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);
                OnCellDisplayChanged(dataItem, e);
            }
        }

        /// <summary>
        /// Sets up multiple headers.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> instance containing the event data.</param>
        /// <param name="dataItems">The data items.</param>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-21
        /// </remarks>
        private void SetUpMultipleHeaders(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex == 0)
            {
                e.Style.CellValue = _groupHeaderText;
                e.Style.Clickable = false;
            }
            else if (e.RowIndex == 1 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;

            }
            else
            {
                T dataItem = dataItems[e.RowIndex - 2];
                e.Style.BackColor = System.Drawing.Color.Gray;
                object cellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);

                if (cellValue == null)
                    e.Style.CellType = "Static";
                else
                    e.Style.ResetBackColor();

                e.Style.CellValue = cellValue;
                OnCellDisplayChanged(dataItem, e);
            }
        }

        #endregion


        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (string.IsNullOrEmpty(_groupHeaderText))
            {
                if (e.ColIndex > 0 && e.RowIndex > 0)
                {
                    if (((string)e.Style.CellValue).Length > _maxLength)
                        return;

                    T dataItem = dataItems[e.RowIndex - 1];
                    _propertyReflector.SetValue(dataItem, _bindingProperty, e.Style.CellValue);
                    OnCellChanged(dataItem, e);
                    e.Handled = true;
                }
            }
            else
            {
                if (e.ColIndex > 0 && e.RowIndex > 1)
                {
                    if (((string)e.Style.CellValue).Length > _maxLength)
                        return;

                    T dataItem = dataItems[e.RowIndex - 2];
                    _propertyReflector.SetValue(dataItem, _bindingProperty, e.Style.CellValue);
                    OnCellChanged(dataItem, e);
                    e.Handled = true;
                }
            }

        }
    }
}