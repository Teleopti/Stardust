using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Shifts;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public class TypeChangeEventArgs:EventArgs
    {
    	public Type NewType { get; set; }

    	public object DataItem { get; set; }
    }

    public class SFGridDropDownTypeColumn<T> : SFGridColumnBase<T>
    {
        private readonly IList<GridClassType> _comboItems;
        private readonly IComparer<T> _columnComparer;

        public event EventHandler<TypeChangeEventArgs> TypeChanged;

        public SFGridDropDownTypeColumn(string headerText, IList<GridClassType> comboItems, IComparer<T> columnComparer)
            : base(string.Empty, headerText)
        {
            _comboItems = comboItems;
            _columnComparer = columnComparer;
        }

        public override IComparer<T> ColumnComparer
        {
            get
            {
                return _columnComparer;
            }
        }


        public SFGridDropDownTypeColumn(string headerText, IList<GridClassType> comboItems)
            : base(string.Empty, headerText)
        {
            _comboItems = comboItems;
        }

        public override int PreferredWidth
        {
            get { return 130; }
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
        	if (e.ColIndex <= 0 || e.RowIndex <= 0) return;
        	T dataItem = dataItems[e.RowIndex - 1];

        	if (e.Style.CellValue is Type)
        	{
        		OnTypeChanged(((Type)e.Style.CellValue), dataItem);
        	}

        	e.Handled = true;
        }

        public virtual void OnTypeChanged(Type newType, T dataItem)
        {
        	if (TypeChanged == null) return;
        	var args = new TypeChangeEventArgs {NewType = newType, DataItem = dataItem};
        	TypeChanged(this, args);
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellType = "ComboBox";
            e.Style.DataSource = _comboItems;
            e.Style.DisplayMember = "Name";
            e.Style.ValueMember = "ClassType";
            e.Style.CellValue = currentItem.GetType();
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            if (e.Style.CellValue is Type)
            {
                OnTypeChanged(((Type)e.Style.CellValue), currentItem);
            }
        }
    }
}