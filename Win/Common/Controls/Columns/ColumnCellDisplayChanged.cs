using System;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
   public class ColumnCellDisplayChangedEventArgs<T>:EventArgs
    {
        private readonly T _dataItem;
       private readonly  GridQueryCellInfoEventArgs _queryCellInfoEventArg;

       public ColumnCellDisplayChangedEventArgs(T dataItem, GridQueryCellInfoEventArgs e)
        {
            _dataItem = dataItem;
            _queryCellInfoEventArg = e;
        }

        public T DataItem
        {
            get { return _dataItem; }
        }

       public GridQueryCellInfoEventArgs QueryCellInfoEventArg
       {
           get { return _queryCellInfoEventArg; }
       }

    }
}
