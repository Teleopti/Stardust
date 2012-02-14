using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.Common;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Win.PeopleAdmin.Controls.Columns
{
    public class NumericCellColumnPeriod<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private string _headerText;
        private string _bindingProperty;
        private int _minValue;

        public NumericCellColumnPeriod(string bindingProperty, string headerText)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
        }

        public NumericCellColumnPeriod(string bindingProperty, string headerText, int minValue)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
            _minValue = minValue;
        }

		public override string BindingProperty
		{
			get
			{
				return _bindingProperty;
			}
		}

        public override int PreferredWidth
        {
            get { return 110; }
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
                e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;
                e.Style.CellValue = _headerText;
            }
            
            if (IsContentRow(e.RowIndex,dataItems.Count))
            {
                T dataItem = dataItems[e.RowIndex - 1];
                object obj = _propertyReflector.GetValue(dataItem, _bindingProperty);

                int value = (int)obj;

                if (value != -1)
                {
                    e.Style.CellValue = obj;
                }
                
                PeopleAdminHelper.GrayColumn(_propertyReflector, dataItem, e);
                OnCellDisplayChanged(dataItem, e);
                e.Style.HorizontalAlignment = GridHorizontalAlignment.Right;
            }
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.ColIndex > 0 && e.RowIndex > 0)
            {
                T dataItem = dataItems[e.RowIndex - 1];

                int value;

                if (!int.TryParse(e.Style.CellValue.ToString(), out value))
                    return;

                if (value >= _minValue && value < 100)
                {
                    _propertyReflector.SetValue(dataItem, _bindingProperty, value);
                    OnCellChanged(dataItem, e);
                }
                e.Handled = true;
            }
        }
    }
}