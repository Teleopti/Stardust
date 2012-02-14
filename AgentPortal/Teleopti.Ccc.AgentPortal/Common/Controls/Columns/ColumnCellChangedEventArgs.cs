using System;

namespace Teleopti.Ccc.AgentPortal.Common.Controls.Columns
{
    public class ColumnCellChangedEventArgs<T> : EventArgs
    {
        private readonly T _dataItem;

        public ColumnCellChangedEventArgs(T dataItem)
        {
            _dataItem = dataItem;
        }

        public T DataItem
        {
            get { return _dataItem; }
        }
    }
}