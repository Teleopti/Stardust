using System.Collections.Generic;
using System.Collections.ObjectModel;

using Syncfusion.Windows.Forms.Grid;

namespace GridTest
{
    public abstract class ScheduleGridColumnBase<T>
    {
        private readonly string _headerText;
        private readonly string _bindingProperty;
        private readonly PropertyReflector _propertyReflectorHelper = new PropertyReflector();

        protected ScheduleGridColumnBase(string bindingProperty, string headerText)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
        }

        public virtual IComparer<T> ColumnComparer
        {
            get { return null; }
        }

        protected string HeaderText
        {
            get { return _headerText; }
        }

        public string BindingProperty
        {
            get { return _bindingProperty; }
        }

        protected PropertyReflector PropertyReflectorHelper
        {
            get { return _propertyReflectorHelper; }
        }

        public virtual void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {

            if (e.RowIndex <= e.Style.CellModel.Grid.Rows.HeaderCount)
            {
                e.Style.CellValue = HeaderText;
            }
            else
            {
                GetCellValue(e, dataItems, dataItems[e.RowIndex - (e.Style.CellModel.Grid.Rows.HeaderCount + 1)]);
            }

            e.Handled = true;
        }

        public abstract void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem);

        public virtual void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex > e.Style.CellModel.Grid.Rows.HeaderCount)
            {
                T dataItem = dataItems[e.RowIndex - (e.Style.CellModel.Grid.Rows.HeaderCount + 1)];
                SaveCellValue(e, dataItems, dataItem);
                e.Handled = true;
            }
        }

        public abstract void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem);

        
    }
}