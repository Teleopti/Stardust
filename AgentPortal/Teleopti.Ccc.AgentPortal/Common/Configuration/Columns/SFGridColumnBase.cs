using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Helper;

namespace Teleopti.Ccc.AgentPortal.Common.Configuration.Columns
{
    public abstract class SFGridColumnBase<T>
    {
        private readonly string _headerText;
        private readonly string _bindingProperty;
        private readonly PropertyReflector _propertyReflectorHelper = new PropertyReflector();
        private int _preferredWidth;

        protected SFGridColumnBase(string bindingProperty, string headerText)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
        }

        protected SFGridColumnBase(string bindingProperty, string headerText, int preferredWidth)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
            _preferredWidth = preferredWidth;
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

        public virtual int PreferredWidth 
        {
            get { return _preferredWidth; }
            set { _preferredWidth = value; } 
        }

        public virtual void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if ((e.Style == null) || (e.Style.CellModel == null))
                return;

            if (e.RowIndex <= e.Style.CellModel.Grid.Rows.HeaderCount)
            {
                e.Style.CellValue = HeaderText;
            }

            else if (dataItems.Count > e.RowIndex - (e.Style.CellModel.Grid.Rows.HeaderCount + 1))
            {
                GetCellValue(e, dataItems, dataItems[e.RowIndex - (e.Style.CellModel.Grid.Rows.HeaderCount + 1)]);
                
                //e.Style.Tag = dataItems[e.RowIndex - (e.Style.CellModel.Grid.Rows.HeaderCount + 1)];
                e.Style.DisplayMember = _bindingProperty;
            }


            e.Handled = true;
        }

        public abstract void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem);

        public virtual void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex > e.Style.CellModel.Grid.Rows.HeaderCount)
            {
                if (dataItems.Count != 0)
                {
                    int row = e.RowIndex - (e.Style.CellModel.Grid.Rows.HeaderCount + 1);
                    if (row < dataItems.Count)
                    {
                        T dataItem = dataItems[row];
                        SaveCellValue(e, dataItems, dataItem);
                        e.Handled = true;    
                    }
                }
            }
        }

        public abstract void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem);
    }
}