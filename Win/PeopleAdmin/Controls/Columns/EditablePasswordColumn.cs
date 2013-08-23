using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.Win.PeopleAdmin.Controls.Columns
{
    class EditablePasswordColumn<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private int _maxLength;
        private string _headerText;
        private bool _readOnly;

        public EditablePasswordColumn(string bindingProperty, int maxLength, string headerText, bool readOnly) : base(bindingProperty, 100)
        {
            _maxLength = maxLength;
            _headerText = headerText;
            _readOnly = readOnly;
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            SetUpSingleHeader(e, dataItems);

            if (_readOnly)
            {
                e.Style.ReadOnly = true;
                e.Style.TextColor = Color.DimGray;
            }
            e.Handled = true;

        }

        /// <summary>
        /// Sets up single header.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> instance containing the event data.</param>
        /// <param name="dataItems">The data items.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-11-05
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
            
                e.Style.CellType = "OriginalTextBox";
                e.Style.PasswordChar = '*';
                e.Style.CharacterCasing = CharacterCasing.Normal;
                e.Style.CellValue = "###########";
                OnCellDisplayChanged(dataItem, e);
            }
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.ColIndex > 0 && e.RowIndex > 0)
            {
                if (((string)e.Style.CellValue).Length > _maxLength)
                    return;

                T dataItem = dataItems[e.RowIndex - 1];
                _propertyReflector.SetValue(dataItem, BindingProperty, e.Style.CellValue);
                OnCellChanged(dataItem, e);
                e.Handled = true;
            }
        }
    }
}