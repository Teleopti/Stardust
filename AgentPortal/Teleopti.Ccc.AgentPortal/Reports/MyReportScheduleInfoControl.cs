using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortal.Common.Configuration.Cells;
using Teleopti.Ccc.AgentPortal.Common.Configuration.Columns;
using Teleopti.Ccc.AgentPortal.Reports.Grid;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortal.Reports
{
    /// <summary>
    /// User control to show the agent's schedule and adherence details
    /// </summary>
    /// <remarks>
    /// Created by: Muhamad Risath
    /// Created date: 10/13/2008
    /// </remarks>
    public partial class MyReportScheduleInfoControl : BaseUserControl
    {
        private IList<MyScheduleGridAdapter> _gridDataSource = new List<MyScheduleGridAdapter>();
        private SFGridColumnGridHelper<MyScheduleGridAdapter> _columnGridHelper;

        public MyReportScheduleInfoControl()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                SetTexts();
            }
        }

        /// <summary>
        /// Initializes the schedule info control.
        /// </summary>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 10/13/2008
        /// </remarks>
        public void InitializeScheduleInfoControl()
        {
            RefreshHeaderDataForDateSelection();
            InitializeScheduleGrid();
        }

        /// <summary>
        /// Refreshes the data for date selection.
        /// </summary>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 10/13/2008
        /// </remarks>
        public void RefreshHeaderDataForDateSelection()
        {
            string titleText = StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson.Name + " " +
                               MyReportControl.StateHolder.SelectedDateTimePeriodDto.LocalStartDateTime.Date.ToShortDateString() + " - " +
                               MyReportControl.StateHolder.SelectedDateTimePeriodDto.LocalEndDateTime.Date.ToShortDateString();

            labelTitle.Text = titleText;
        }

        /// <summary>
        /// Initializes the schedule grid.
        /// </summary>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 10/15/2008
        public void InitializeScheduleGrid()
        {
            bindForGrid();
            gridControlMyWeekSchedules.Invalidate();
        }

        private TimePeriod calculateTimePeriod()
        {
            TimeSpan min = TimeSpan.MaxValue;
            TimeSpan max = TimeSpan.MinValue;

            foreach (MyScheduleGridAdapter gridAdapter in _gridDataSource)
            {
                TimePeriod? period = gridAdapter.MyScheduleAdherence.Period();
                if(period.HasValue)
                {
                    if (period.Value.StartTime < min)
                        min = period.Value.StartTime;
                    if (period.Value.EndTime > max)
                        max = period.Value.EndTime;
                }
            }

            if (min == TimeSpan.MaxValue)
                return new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(18));

            return new TimePeriod(min, max);
        }

        /// <summary>
        /// Binds for grid.
        /// </summary>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 10/16/2008
        /// </remarks>
        private void bindForGrid()
        {
            _gridDataSource = MyReportControl.StateHolder.MyScheduleGridAdapterCollection;
			//bind for the grid.
            ReadOnlyCollection<SFGridColumnBase<MyScheduleGridAdapter>> configGrid = configureGrid();
            _columnGridHelper =
                new SFGridColumnGridHelper<MyScheduleGridAdapter>(gridControlMyWeekSchedules, configGrid,
                                                                  _gridDataSource);
            
            gridControlMyWeekSchedules.RowHeights[0] = 30;
            gridControlMyWeekSchedules.RowCount = _gridDataSource.Count;
            gridControlMyWeekSchedules.Dock = DockStyle.Fill;
        }

        /// <summary>
        /// Configures the grid.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 10/16/2008
        /// </remarks>
        private ReadOnlyCollection<SFGridColumnBase<MyScheduleGridAdapter>> configureGrid()
        {
            if (!gridControlMyWeekSchedules.CellModels.ContainsKey("VisualProjectionColumnHeaderCell"))
                gridControlMyWeekSchedules.CellModels.Add("VisualProjectionColumnHeaderCell",
                                                          new VisualProjectionColumnHeaderCellModel(
                                                              gridControlMyWeekSchedules.Model));
            if (!gridControlMyWeekSchedules.CellModels.ContainsKey("ScheduleAdherenceCell"))
                gridControlMyWeekSchedules.CellModels.Add("ScheduleAdherenceCell",
                                                          new ScheduleAdherenceCellModel(
                                                              gridControlMyWeekSchedules.Model));
            gridControlMyWeekSchedules.DefaultRowHeight = 40;

            IList<SFGridColumnBase<MyScheduleGridAdapter>> gridColumns =
                new List<SFGridColumnBase<MyScheduleGridAdapter>>();

            gridControlMyWeekSchedules.Rows.HeaderCount = 0;
            // Grid must have a Header column
            gridColumns.Add(new SFGridRowHeaderColumn<MyScheduleGridAdapter>(string.Empty));

            gridColumns.Add(
                new SFGridStringColumn<MyScheduleGridAdapter>("ShortDateTime", UserTexts.Resources.Date, 110));

            gridColumns.Add(new SFGridStringColumn<MyScheduleGridAdapter>("Adherence", UserTexts.Resources.Adherence, 80));

            int width = gridControlMyWeekSchedules.Width - 200;
            SFGridScheduleAdherenceColumn<MyScheduleGridAdapter> adherenceColumn =
                new SFGridScheduleAdherenceColumn<MyScheduleGridAdapter>("MyScheduleAdherence", " ", width);
            gridControlMyWeekSchedules.ColWidths[3] = width;
            gridControlMyWeekSchedules.Invalidate();
			adherenceColumn.TimePeriod = calculateTimePeriod();

            gridColumns.Add(adherenceColumn);

            gridControlMyWeekSchedules.ColCount = gridColumns.Count - 1; //col index starts on 0

            gridControlMyWeekSchedules.ColHiddenEntries.Add(new GridColHidden(0));
            //gridControlMyWeekSchedules.Cols.FreezeRange(0, 0);

            return new ReadOnlyCollection<SFGridColumnBase<MyScheduleGridAdapter>>(gridColumns);
        }

        private void gridControlMyWeekSchedules_Resize(object sender, EventArgs e)
        {
            if ((gridControlMyWeekSchedules.Width - 200)>0)
                gridControlMyWeekSchedules.ColWidths[3] = gridControlMyWeekSchedules.Width - 200;
        }
    }
}