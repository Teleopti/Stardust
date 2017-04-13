using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns
{
    public interface IColumn<T>
    {
        event EventHandler<ColumnCellChangedEventArgs<T>> CellChanged;
        event EventHandler<ColumnCellDisplayChangedEventArgs<T>> CellDisplayChanged;

        IComparer<T> ColumnComparer { get; }

        int PreferredWidth { get; }

        void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems);
        void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems);
    }
}