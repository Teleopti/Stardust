using System.Collections.ObjectModel;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.Win.PeopleAdmin.Controls.Columns
{
    public class ReadOnlyTextColumnPeriod<T> : ColumnBase<T>
    {
        private static int _preferredWidth = 110;

        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private string _headerText;
        private string _bindingProperty;
        private bool _makeRightAlign;


        public ReadOnlyTextColumnPeriod(string bindingProperty, string headerText, bool makeRightAlign)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
            _makeRightAlign = makeRightAlign;
        }

        public override int PreferredWidth
        {
            get { return _preferredWidth; }
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
                if (dataItems.Count == 0) return;
                e.Style.ReadOnly = true;
                T dataItem = dataItems[e.RowIndex - 1];

                e.Style.CellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);
                
                PeopleAdminHelper.GrayColumn(_propertyReflector, dataItem, e);
              
                e.Style.TextColor = Color.DimGray;
                e.Style.HorizontalAlignment = (_makeRightAlign) ? GridHorizontalAlignment.Right : GridHorizontalAlignment.Left;
                e.Handled = true;
            }
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            e.Handled = true;
        }
    }
}
