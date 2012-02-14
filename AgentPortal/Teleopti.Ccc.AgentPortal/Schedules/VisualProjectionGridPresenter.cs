using System;
using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Common.Configuration.Cells;
using Teleopti.Ccc.AgentPortal.Common.Configuration.Rows;
using Teleopti.Ccc.AgentPortal.Common.Controls.Columns;
using Teleopti.Ccc.AgentPortalCode.AgentSchedule.Comparers;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortal.Schedules
{
    public class VisualProjectionGridPresenter
    {
        private readonly GridControl _grid;
        private IList<ScheduleGridColumnBase<VisualProjection>> _gridColumns;
        private readonly IList<VisualProjection> _schedules;

        public VisualProjectionGridPresenter(GridControl grid, IList<VisualProjection> schedules)
        {
            _grid = grid;
            _schedules = schedules;
            configureGrid();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public IList<ScheduleGridColumnBase<VisualProjection>> GridColumns
        {
            get { return _gridColumns; }
        }

        private VisualProjectionColumnHeaderCellModel initializeVisualProjectionColumnHeaderCell()
        {
            VisualProjectionColumnHeaderCellModel cellModel = new VisualProjectionColumnHeaderCellModel(_grid.Model);
            return cellModel;
        }

        private VisualProjectionCellModel initializeVisualProjectionCell()
        {
            VisualProjectionCellModel cellModel = new VisualProjectionCellModel(_grid.Model);
            return cellModel;
        }
        private TimePeriod DisplayedPeriod()
        {
            TimeSpan minStart = TimeSpan.MaxValue;
            TimeSpan maxEnd = TimeSpan.MinValue;
            foreach (VisualProjection visualProjection in _schedules)
            {
                var period = visualProjection.Period();
                if (period.HasValue)
                {
                    if (period.Value.StartTime < minStart)
                        minStart = period.Value.StartTime;
                    if (period.Value.EndTime > maxEnd)
                        maxEnd = period.Value.EndTime;
                }
            }
            if (_schedules.Count == 0)
            {
                minStart = new TimeSpan(0,8,0);
                maxEnd = new TimeSpan(0,17,0);
            }
            if (minStart == TimeSpan.MaxValue && maxEnd == TimeSpan.MinValue)
            {
                minStart = new TimeSpan(0, 8, 0);
                maxEnd = new TimeSpan(0, 17, 0);
            }
            return new TimePeriod(minStart.Add(TimeSpan.FromHours(-1)), maxEnd.Add(TimeSpan.FromHours(1)));
        }

        private void configureGrid()
        {
            _gridColumns = new List<ScheduleGridColumnBase<VisualProjection>>();
            _grid.Rows.HeaderCount = 0;

            if (!_grid.CellModels.ContainsKey("VisualProjectionColumnHeaderCell"))
                _grid.CellModels.Add("VisualProjectionColumnHeaderCell", initializeVisualProjectionColumnHeaderCell());
            if (!_grid.CellModels.ContainsKey("VisualProjectionCell"))
                _grid.CellModels.Add("VisualProjectionCell", initializeVisualProjectionCell());
            // Grid must have a Header column
            var nameColumn = new VisualProjectionGridRowHeaderColumn();
            nameColumn.ColumnComparer = new TeamAgentNameComparer();
            GridColumns.Add(nameColumn);
            ScheduleGridVisualProjectionColumn scheduleColumn =
                new ScheduleGridVisualProjectionColumn("LayerCollection", String.Empty, new TeamScheduleComparer());
            scheduleColumn.Period = DisplayedPeriod();
            GridColumns.Add(scheduleColumn);

            _grid.RowCount = _schedules.Count + _grid.Rows.HeaderCount; //add number of headers, 1 header = 0, 2 headers = 1 ...
            _grid.ColCount = GridColumns.Count - 1;  //col index starts on 0
            _grid.DefaultRowHeight = 25;
            _grid.RowHeights.SetSize(0, 35);

            _grid.CurrentCell.MoveTo(1, 1, GridSetCurrentCellOptions.SetFocus);

        }
    }
}