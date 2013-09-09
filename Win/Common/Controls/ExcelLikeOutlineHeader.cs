using System.Drawing;
using Syncfusion.ComponentModel;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Controls
{
    /// <summary>
    /// Highlight column and row headers for selected range
    /// </summary>
    public class ExcelLikeOutlineHeader : Disposable
    {
        GridControlBase m_grid;
        GridRangeInfo activeRange;

        public ExcelLikeOutlineHeader(GridControlBase grid)
        {
            m_grid = grid;

            this.m_grid.Model.SelectionChanged += gridSelectionChanged;
            this.m_grid.PrepareViewStyleInfo += gridPrepareViewStyleInfo;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.m_grid.Model.SelectionChanged -= gridSelectionChanged;
                this.m_grid.PrepareViewStyleInfo -= gridPrepareViewStyleInfo;
            }

            base.Dispose(disposing);
        }

        private static GridRangeInfo GetColHeaderRange(GridRangeInfo range)
        {
            return GridRangeInfo.Cells(0, range.Left, 0, range.Right);
        }

        private static GridRangeInfo GetRowHeaderRange(GridRangeInfo range)
        {
            return GridRangeInfo.Cells(range.Top, 0, range.Bottom, 0);
        }


        /// <summary>
        /// Repaint selected cells.
        /// </summary>
        void gridSelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            GridRangeInfo colRange = GridRangeInfo.Empty;
            GridRangeInfo rowRange = GridRangeInfo.Empty;

            if (activeRange != null && !activeRange.IsEmpty)
            {
                colRange = GetColHeaderRange(activeRange);
                rowRange = GetRowHeaderRange(activeRange);
            }

            activeRange = e.Range;

            if (activeRange != null && !activeRange.IsEmpty)
            {
                colRange = GridRangeInfo.UnionRange(colRange, GetColHeaderRange(activeRange));
                rowRange = GridRangeInfo.UnionRange(rowRange, GetRowHeaderRange(activeRange));
            }

            m_grid.UpdateWithDrawClippedGrid(m_grid.RangeInfoToRectangle(colRange));
            m_grid.UpdateWithDrawClippedGrid(m_grid.RangeInfoToRectangle(rowRange));
        }

        private void gridPrepareViewStyleInfo(object sender, GridPrepareViewStyleInfoEventArgs e)
        {
            GridBorder gb = new GridBorder(GridBorderStyle.Solid, Color.FromArgb(242, 149, 54));
            if (activeRange != null)
            {
                if (((e.RowIndex == 0 && (e.ColIndex >= activeRange.Left && e.ColIndex <= activeRange.Right))
                    || (e.ColIndex == 0 && (e.RowIndex >= activeRange.Top && e.RowIndex <= activeRange.Bottom)))
                    && !(e.ColIndex == 0 && e.RowIndex == 0))
                {
                    e.Style.Borders.Bottom = e.Style.Borders.Right = gb;
                    if (e.RowIndex == 0)
                        e.Style.Interior = new BrushInfo(GradientStyle.Vertical, Color.FromArgb(249, 217, 159), Color.FromArgb(242, 193, 96));
                    else if (e.ColIndex == 0)
                        e.Style.BackColor = Color.FromArgb(255, 213, 141);
                    e.Cancel = true;
                }
            }
        }
    }
}
