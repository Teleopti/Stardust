using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.Common;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.PeopleAdmin.Controls.Columns
{
    public class PercentageCellColumnForSchedulePeriod<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private string _headerText;
        private string _bindingProperty;

        public PercentageCellColumnForSchedulePeriod(string bindingProperty, string headerText)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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

                e.Style.CellType = GridCellModelConstants.CellTypePercentCell;

                T dataItem = dataItems[e.RowIndex - 1];
                object obj = _propertyReflector.GetValue(dataItem, _bindingProperty);

                Percent value = (Percent)obj;

                e.Style.CellValue = value;

                PeopleAdminHelper.GrayColumn(_propertyReflector, dataItem, e);
                OnCellDisplayChanged(dataItem, e);
                e.Style.HorizontalAlignment = GridHorizontalAlignment.Right;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.ColIndex > 0 && e.RowIndex > 0)
            {
                T dataItem = dataItems[e.RowIndex - 1];

                Percent value = new Percent(0);

                if (string.IsNullOrEmpty(e.Style.CellValue.ToString()))
                    return;
                if (!Percent.TryParse(e.Style.CellValue.ToString(), out value))
                    return;
                if (value.Value < -1 || value.Value > 1)
                    return;
                
                _propertyReflector.SetValue(dataItem, _bindingProperty, value);
                OnCellChanged(dataItem, e);
                e.Handled = true;
            }
        }
    }
}