using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Controls.Columns
{
    public class EditableNotEmptyTextColumn<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private readonly int _maxLength;
        private readonly string _headerText;
    	private readonly bool _stripTooLongText;
        
		public EditableNotEmptyTextColumn(string bindingProperty, int maxLength, string headerText, bool stripTooLongText) : base(bindingProperty,100)
		{
			_maxLength = maxLength;
			_headerText = headerText;
			_stripTooLongText = stripTooLongText;
		}

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            SetUpSingleHeader(e, dataItems);
            e.Handled = true;
        }

        private void SetUpSingleHeader(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
            }
            if (IsContentRow(e.RowIndex,dataItems.Count))
            {
                T dataItem = dataItems[e.RowIndex - 1];
                e.Style.CellValue = _propertyReflector.GetValue(dataItem, BindingProperty);
                OnCellDisplayChanged(dataItem, e);
            }
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.ColIndex > 0 && e.RowIndex > 0)
            {
				if(_stripTooLongText && ((string) e.Style.CellValue).Length > _maxLength)
					e.Style.CellValue = ((string) e.Style.CellValue).Substring(0, _maxLength);

                if (((string) e.Style.CellValue).Length > _maxLength || ((string) e.Style.CellValue).Length == 0)
                    return;

                T dataItem = dataItems[e.RowIndex - 1];
                _propertyReflector.SetValue(dataItem, BindingProperty, e.Style.CellValue);
                OnCellChanged(dataItem, e);
                e.Handled = true;
            }
        }
    }
}