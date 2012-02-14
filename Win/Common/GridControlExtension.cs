using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common
{
    public static class GridControlExtension
    {
        public static void ResizeToFit(this GridControl grid)
        {
            grid.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
        }
    }
}
