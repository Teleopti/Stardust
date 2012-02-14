using System;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
    public class TimeOfDayColumn<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private string _headerText;
        private string _groupHeaderText;
        private string _bindingProperty;

        public TimeOfDayColumn(string bindingProperty, string headerText)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
        }

        public TimeOfDayColumn(string bindingProperty, string headerText, string groupHeaderText)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
            _groupHeaderText = groupHeaderText;
        }

        public override int PreferredWidth
        {
            get { return 150; }
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
                e.Style.CellType = "TimeSpanTimeOfDayCellModel";
                T dataItem = dataItems[e.RowIndex - 1];
                e.Style.CellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);
                InvokeValidate(dataItem, e.Style, e.RowIndex, false);
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
            }
            else if (e.RowIndex == 1 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
            }
            else
            {
                e.Style.BackColor = System.Drawing.Color.Gray;
                if (dataItems.Count == 0)
                    return;

                T dataItem = dataItems[e.RowIndex - 2];
                object cellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);
                e.Style.CellType = "TimeSpanTimeOfDayCellModel";
                if (cellValue == null)
                    e.Style.CellType = "Static";
                else
                    e.Style.ResetBackColor();
                e.Style.CellValue = cellValue;
                InvokeValidate(dataItem, e.Style, e.RowIndex, false);
            }
        }

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.TimeSpan.TryParse(System.String,System.TimeSpan@)")]
        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            int rowIndex = 0;
            if (string.IsNullOrEmpty(_groupHeaderText))
            {
                if (e.ColIndex > 0 && e.RowIndex > 0)
                {
                    rowIndex = e.RowIndex - 1;
                }
            }
            else
            {
                if (e.ColIndex > 0 && e.RowIndex > 1)
                {
                    rowIndex = e.RowIndex - 2;
                }
            }
            T dataItem = dataItems[rowIndex];
            if (e.Style.CellValue != null)
            {
                TimeSpan cellValue;
                TimeSpan.TryParse(e.Style.CellValue.ToString(), out cellValue);
                _propertyReflector.SetValue(dataItem, _bindingProperty, cellValue);
            }

            InvokeValidate(dataItem, e.Style, e.RowIndex, true);

            OnCellChanged(dataItem);
            e.Handled = true;

        }

        /// <summary>
        /// Invokes the validate.
        /// </summary>
        /// <param name="dataItem">The data item.</param>
        /// <param name="style">The style.</param>
        /// <param name="rowIndex">Index of the row.</param>
        /// <param name="inSaveMode">if set to <c>true</c> [in save mode].</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-10-02
        /// </remarks>
        private void InvokeValidate(T dataItem, GridStyleInfo style, int rowIndex, bool inSaveMode)
        {
        	var handler = Validate;
            if (handler != null)
            {
                IAsyncResult result = handler.BeginInvoke(dataItem, style, rowIndex, inSaveMode, null, null);
                handler.EndInvoke(result);
            }
        }
    }
}