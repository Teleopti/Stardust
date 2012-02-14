using System;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.AgentPortal.Common.Controls.Columns
{
    public interface IColumn<T>
    {
        event EventHandler<ColumnCellChangedEventArgs<T>> CellChanged;

        int PreferredWidth { get; }

        void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems);

        void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems);

        void CellsChanging(GridCellsChangingEventArgs e);
    }
}