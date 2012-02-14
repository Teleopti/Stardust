using System.Collections.ObjectModel;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
    /// <summary>
    /// Read only text column 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by:SanjayaI
    /// Created date: 5/27/2008
    /// </remarks>
    public class ReadOnlyTextColumn<T> : ColumnBase<T>
    {
        private int _preferredWidth = 110;

        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private string _headerText;
        private string _bindingProperty;
        private string _groupHeaderText;
        private bool _makeStatic;
        

        public ReadOnlyTextColumn(string bindingProperty, string headerText, params bool[] makeStatic)
        {

            _headerText = headerText;
            _bindingProperty = bindingProperty;

            SetStaticBehaviour( ((makeStatic != null) && (makeStatic.Length == 1) && (makeStatic[0])) ? true : false);
        }

        public ReadOnlyTextColumn(string bindingProperty, string headerText, string groupHeaderText, params bool[] makeStatic)
            : this(bindingProperty, headerText, makeStatic)
        {
            _groupHeaderText = groupHeaderText;
        }

        public void SetPreferredWidth(int width)
        {
            _preferredWidth = width;
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
            if (string.IsNullOrEmpty(_groupHeaderText))
            {
                SetUpSingleHeader(e, dataItems);
            }
            else
            {
                SetUpMultipleHeaders(e, dataItems);
            }
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
            else
            {
                if (dataItems.Count == 0 || dataItems.Count <= (e.RowIndex - 1)) return;
                T dataItem = dataItems[e.RowIndex - 1];
                e.Style.CellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);

                e.Style.ReadOnly = true;
                e.Style.CellType = (_makeStatic) ? "Static" : e.Style.CellType;
                e.Style.TextColor = Color.DimGray;

                OnCellDisplayChanged(dataItem, e);

                e.Handled = true;
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
                if (dataItems.Count == 0) return;
                T dataItem = dataItems[e.RowIndex - 2];
                e.Style.CellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);

                e.Style.ReadOnly = true;
                e.Style.CellType = (_makeStatic) ? "Static" : e.Style.CellType;
                e.Style.TextColor = Color.DimGray;
                OnCellDisplayChanged(dataItem, e);
                e.Handled = true;
            }
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            e.Handled = true;
        }

        private void SetStaticBehaviour(bool makeStatic)
        {
            _makeStatic = makeStatic;
        }
    }
}