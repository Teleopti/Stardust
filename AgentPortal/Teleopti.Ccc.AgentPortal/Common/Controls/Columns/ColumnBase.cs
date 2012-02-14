using System;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.AgentPortal.Common.Controls.Columns
{
    public delegate void Validate<T>(T dataItem, GridStyleInfo styleInfo, int row, bool inSaveMode);

    public abstract class ColumnBase<T> : IColumn<T>
    {
        #region IColumn<T> Members

        public event EventHandler<ColumnCellChangedEventArgs<T>> CellChanged;

        public abstract int PreferredWidth { get; }

        public virtual string BindingProperty
        {
            get { return string.Empty; }
        }

        private Validate<T> valiadate;

        public Validate<T> Validate
        {
            get { return valiadate; }
            set { valiadate = value; }
        }

        public abstract void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems);

        public abstract void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems);

        public virtual void OnCellChanged(T dataItem)
        {
            if (CellChanged != null)
            {
                ColumnCellChangedEventArgs<T> args = new ColumnCellChangedEventArgs<T>(dataItem);
                CellChanged(this, args);
            }
        }

        public virtual void CellsChanging(GridCellsChangingEventArgs e)
        {
        }

        #endregion
    }
}