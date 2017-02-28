using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common
{
    public class GridSchedulesExtractor : IGridSchedulesExtractor
    {
        private readonly GridControl _grid;

        public GridSchedulesExtractor(GridControl gridControl)
        {
            _grid = gridControl;
        }

        public IList<IScheduleDay> Extract()
        {
            return CopyAllSchedules();
        }

        public IList<IScheduleDay> ExtractSelected()
        {
            return CopySelectedSchedules();
        }

        private IList<IScheduleDay> CopyAllSchedules()
        {
            var clipHandler = new ClipHandler<ExtractedSchedule>();

            GridCopyAll(clipHandler);

            return clipHandler.ClipList.Select(clip => clip.ClipValue).Cast<IScheduleDay>().ToList();
        }

        private IList<IScheduleDay> CopySelectedSchedules()
        {
            var clipHandler = new ClipHandler<ExtractedSchedule>();
            GridHelper.GridCopySelection(_grid, clipHandler, true);
            return clipHandler.ClipList.Select(clip => clip.ClipValue).Cast<IScheduleDay>().ToList();
        }


        private void GridCopyAll<T>(ClipHandler<T> clipHandler)
        {
            const int top = 0;
            const int left = 0;
            var right = _grid.ColCount;
            var bottom = _grid.RowCount;

            clipHandler.Clear();

            for (var row = top; row <= bottom; row++)
            {
                for (var col = left; col <= right; col++)
                {
                    var cellValue = _grid.Model[row, col].CellValue;
                    if (!(cellValue is T)) continue;
                    if ((row <= _grid.Rows.HeaderCount || col <= _grid.Cols.HeaderCount))
                    {
                        continue;
                    }

                    clipHandler.AddClip(row, col, (T)cellValue);
                }
            }
        }

        
    }
}
