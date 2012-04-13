using System;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.Win.PeopleAdmin.Controls.Columns
{
    class EditableEmailColumn<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private int _maxLength;
        private string _headerText;
        private string _bindingProperty;

        public EditableEmailColumn(string bindingProperty, int maxLength, string headerText)
        {
            _maxLength = maxLength;
            _headerText = headerText;
            _bindingProperty = bindingProperty;
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
            SetUpSingleHeader(e, dataItems);
            e.Handled = true;
        }

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

            if (IsContentRow(e.RowIndex,dataItems.Count))
            {
                T dataItem = dataItems[e.RowIndex - 1];
                e.Style.CellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);
                OnCellDisplayChanged(dataItem, e);
            }
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.ColIndex > 0 && e.RowIndex > 0)
            {
                if (((string) e.Style.CellValue).Length > _maxLength)
                    return;
                //check IsEmail
                if (!IsEmail((string) e.Style.CellValue)) return;

                T dataItem = dataItems[e.RowIndex - 1];
                _propertyReflector.SetValue(dataItem, _bindingProperty, e.Style.CellValue);
                OnCellChanged(dataItem, e);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Determines whether the specified input email is email.
        /// </summary>
        /// <param name="inputEmail">The input email.</param>
        /// <returns>
        /// 	<c>true</c> if the specified input email is email; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-06-01
        /// </remarks>
        private static bool IsEmail(string inputEmail)
        {
            if (!String.IsNullOrEmpty(inputEmail))
            {
            	return inputEmail.Contains("@");
            }
            return true;
        }
    }
}