using System;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
    public class ColumnCellChangedEventArgs<T> : EventArgs
    {
        private readonly T _dataItem;
        private readonly GridSaveCellInfoEventArgs _saveCellInfoEventArgs;

        public ColumnCellChangedEventArgs(T dataItem)
        {
            _dataItem = dataItem;
        }

        public ColumnCellChangedEventArgs(T dataItem, GridSaveCellInfoEventArgs e)
        {
            _dataItem = dataItem;
            _saveCellInfoEventArgs = e;
        }

        public T DataItem
        {
            get { return _dataItem; }
        }

        public GridSaveCellInfoEventArgs SaveCellInfoEventArgs
        {
            get
            {
            	if (_saveCellInfoEventArgs != null) 
						 return _saveCellInfoEventArgs;
            	return null;
            }
        }

    }
}