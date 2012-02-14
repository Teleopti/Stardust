using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Helper;

namespace Teleopti.Ccc.AgentPortal.Common.Controls.Columns
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
            get; set;
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

        public virtual void GetCellInfo(GridQueryCellInfoEventArgs e, IList<T> dataItems)
        {
            if (e.RowIndex <= e.Style.CellModel.Grid.Rows.HeaderCount)
            {
                e.Style.CellValue = HeaderText;
            }
            else
            {
                int index = e.RowIndex - (e.Style.CellModel.Grid.Rows.HeaderCount + 1);
                if (index<dataItems.Count)
                    GetCellValue(e, dataItems, dataItems[index]);
            }
            e.Handled = true;
        }

        public abstract void GetCellValue(GridQueryCellInfoEventArgs e, IList<T> dataItems, T currentItem);

        public virtual void SaveCellInfo(GridSaveCellInfoEventArgs e, IList<T> dataItems)
        {
            if (e.RowIndex > e.Style.CellModel.Grid.Rows.HeaderCount)
            {
                T dataItem = dataItems[e.RowIndex - (e.Style.CellModel.Grid.Rows.HeaderCount + 1)];
                SaveCellValue(e, dataItems, dataItem);
                e.Handled = true;
            }
        }

        public abstract void SaveCellValue(GridSaveCellInfoEventArgs e, IList<T> dataItems, T currentItem);


    }
}