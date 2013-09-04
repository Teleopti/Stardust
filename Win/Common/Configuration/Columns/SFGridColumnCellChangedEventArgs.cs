using System;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public class SFGridColumnCellChangedEventArgs<T> : EventArgs
    {
        private readonly T _dataItem;

        public SFGridColumnCellChangedEventArgs(T dataItem)
        {
            _dataItem = dataItem;
        }

        public T DataItem
        {
            get { return _dataItem; }
        }

    }
}