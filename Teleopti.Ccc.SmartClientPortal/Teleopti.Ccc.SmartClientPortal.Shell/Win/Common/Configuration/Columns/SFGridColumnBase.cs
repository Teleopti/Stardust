using System.Collections.Generic;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns
{
    public abstract class SFGridColumnBase<T> : ISortColumn<T>
    {
        protected SFGridColumnBase(string bindingProperty, string headerText)
        {
            HeaderText = headerText;
            BindingProperty = bindingProperty;
            PropertyReflectorHelper = new PropertyReflector();
        }

		public bool AllowEmptyValue { get; set; }

        public SortCompare<T> SortCompare { get; set; }

        public virtual IComparer<T> Comparer
        {
            get
            {
                IComparer<T> comparer = null;
                if (SortCompare != null)
                    comparer = new ComparerBase<T>(SortCompare);
                return comparer;
            }
        }

        protected string GroupHeaderText { get; set; }

        public bool? IsAscending { get; set; }

        protected string HeaderText { get; private set; }
 
        public string BindingProperty { get; private set; }

        protected PropertyReflector PropertyReflectorHelper { get; private set; }

        public ISFGridCellValidatorBase<T> CellValidator { get; set; }

        public abstract int PreferredWidth { get; }

        public virtual void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex == 0)
            {
                if (!string.IsNullOrEmpty(GroupHeaderText))
                {
                    e.Style.CellValue = GroupHeaderText;
                    e.Style.MergeCell = GridMergeCellDirection.ColumnsInRow;
                }
                else
                {
                    e.Style.CellValue = HeaderText;
                }
            }
            else if (e.RowIndex <= e.Style.CellModel.Grid.Rows.HeaderCount)
            {
                e.Style.CellValue = HeaderText;
            }
            else
            {
                // Gets the current item.
                if (dataItems.Count >= e.RowIndex - (e.Style.CellModel.Grid.Rows.HeaderCount))
                {
                    T dataItem = dataItems[e.RowIndex - (e.Style.CellModel.Grid.Rows.HeaderCount + 1)];

                    // Displays the current item.
                    GetCellValue(e, dataItems, dataItem);
                    // Validates & verifies the value.
                    if (CellValidator != null)
                    {
                        CellValidator.ValidateCell(e, e.Style, dataItem);
                    }
                    // Converts to an ignorecell (if needed).
                    ApplyIgnoreCell(e.Style);  
                }
                
            }

            e.Handled = true;
        }

        protected virtual void ApplyIgnoreCell(GridStyleInfo style)
        {
        	if (style.CellValue == null) return;
        	style.CellValueType = style.CellValue.GetType();
        	if (style.CellValueType == typeof(Ignore))
        	{
        		style.CellType = "IgnoreCell";
        	}
        }

        public abstract void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem);

        public virtual void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
        	if (e.RowIndex <= e.Style.CellModel.Grid.Rows.HeaderCount) return;
        	// Gets the current item.
        	T dataItem = dataItems[e.RowIndex - (e.Style.CellModel.Grid.Rows.HeaderCount + 1)];
        	// Saves validated values in the cell.
        	SaveCellValue(e, dataItems, dataItem);

        	e.Handled = true;
        }

        public abstract void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem);
    }
}