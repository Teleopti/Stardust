using System.Drawing;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns
{
    public interface ISFGridCellValidatorBase<T>
    {
        Color ErrorBackColor { get; }
        bool Canceled { get; set; }
        string Message { get; set; }
        int RowIndex { get; }
        int ColIndex { get; }
        void ValidateCell(GridCellHandledEventArgs e, GridStyleInfo style, T dataItem);
        bool ValidateCell(T dataItem);
    }
}