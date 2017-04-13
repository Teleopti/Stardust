using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{
    public static class GridControlExtension
    {
        public static void ResizeToFit(this GridControl grid)
        {
            grid.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
        }
    }
}
