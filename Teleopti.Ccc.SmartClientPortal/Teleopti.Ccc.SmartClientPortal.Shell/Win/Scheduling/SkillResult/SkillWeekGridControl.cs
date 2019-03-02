using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.SkillResult;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SkillResult
{
	public class SkillWeekGridControl : SkillResultGridControlBase, ISkillWeekGridControl
    {
        private const int rowHeaderWidth = 200;
        private const string settingName = "SchedulerSkillWeekGridAndChart";
		private RowManagerScheduler<SkillWeekGridRow, IDictionary<DateOnlyPeriod, IList<ISkillStaffPeriod>>> _rowManager;
		private readonly SkillWeekGridControlPresenter _presenter;

        public SkillWeekGridControl(ITimeZoneGuard timeZoneGuard)
        {
            initializeComponent();
            initializeGrid();
            InitializeBase(settingName, timeZoneGuard);

			_presenter = new SkillWeekGridControlPresenter(this);
        }

        private void initializeComponent()
        {
            QueryColWidth += gridSkillDataQueryColWidth;
            QueryCellInfo += gridSkillDataQueryCellInfo;
            Model.ClipboardCanCopy +=Model_ClipboardCanCopy;

            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            ResumeLayout(false);
        }

        private void Model_ClipboardCanCopy(object sender, GridCutPasteEventArgs e)
        {
            e.Result = true;
            e.Handled = true;
        }

        private void initializeGrid()
        {
            CellModels.Add("TimeCell", timeSpanLongHourMinutesStaticCellModel());
            CellModels.Add("TimeSpanCell", initializeCallTimeSpanCell());
            CellModels.Add("ReadOnlyPercentCell", initializeCallPercentReadOnlyCell());
        }

        private GridCellModelBase timeSpanLongHourMinutesStaticCellModel()
        {
            return new TimeSpanDurationStaticCellModel(Model);
        }

        private GridCellModelBase initializeCallTimeSpanCell()
        {
            return new TimeSpanDurationStaticCellModel(Model);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private GridCellModelBase initializeCallPercentReadOnlyCell()
        {
			return new PercentReadOnlyCellModel(Model) { NumberOfDecimals = 1 };
        }

        private void gridSkillDataQueryColWidth(object sender, GridRowColSizeEventArgs e)
        {	
			e.Size = rowHeaderWidth;
            e.Handled = true;   
        }

        private void gridSkillDataQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
        {
            if (e.ColIndex < 0 || e.RowIndex < 0) return;
			if (GridRows == null)
				return;
            if (e.ColIndex == 0 && e.RowIndex == 1) e.Style.CellValue = " ";

			if (e.RowIndex < GridRows.Count)
				GridRows[e.RowIndex].QueryCellInfo(GetCellInfo(e.Style, e.ColIndex, e.RowIndex));
            if (e.ColIndex > 0) e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;

            e.Handled = true;
        }

        public void CreateGridRows(ISkill skill, IList<DateOnly> dates, ISchedulerStateHolder schedulerStateHolder)
        {
			if (skill == null || dates == null) return;

            ((NumericReadOnlyCellModel)CellModels["NumericReadOnlyCell"]).NumberOfDecimals = 2;

	        GridRows = new List<IGridRow> { new DateHeaderGridRow(DateHeaderType.WeekDates, dates) };
        	DateOnly baseDate = dates.Count > 0 ? dates.First() : DateOnly.MinValue;

            _rowManager = new RowManagerScheduler<SkillWeekGridRow, IDictionary<DateOnlyPeriod, IList<ISkillStaffPeriod>>>(this, new List<IntervalDefinition>(), 15, schedulerStateHolder)
	            {BaseDate = baseDate.Date};

	        SkillWeekGridRow gridRow;

			if (skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill)
			{
				gridRow = new SkillWeekGridRow(_rowManager, "TimeCell", "ForecastedHours", UserTexts.Resources.ForecastedHours);
				gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
				GridRows.Add(_rowManager.AddRow(gridRow));

				if (!skill.IsVirtual)
				{
					gridRow = new SkillWeekGridRowMinMaxIssues(_rowManager, "TimeCell", "ScheduledHours", UserTexts.Resources.ScheduledHours);
					gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
					GridRows.Add(_rowManager.AddRow(gridRow));
				}
				else
				{
					gridRow = new SkillWeekGridRowMinMaxIssuesSummary(_rowManager, "TimeCell", "ScheduledHours", UserTexts.Resources.ScheduledHours);
					gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
					GridRows.Add(_rowManager.AddRow(gridRow));
				}

				gridRow = new SkillWeekGridRow(_rowManager, "TimeSpanCell", "AbsoluteDifference", UserTexts.Resources.AbsoluteDifference);
				gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
				GridRows.Add(_rowManager.AddRow(gridRow));

				if (!skill.IsVirtual)
				{
					gridRow = new SkillWeekGridRowStaffingIssues(_rowManager, "ReadOnlyPercentCell", "RelativeDifference",UserTexts.Resources.RelativeDifference, skill);
					gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
					GridRows.Add(_rowManager.AddRow(gridRow));
				}
				else
				{
					gridRow = new SkillWeekGridRowStaffingIssuesSummary(_rowManager, "ReadOnlyPercentCell", "RelativeDifference",UserTexts.Resources.RelativeDifference);
					gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
					GridRows.Add(_rowManager.AddRow(gridRow));
				}

				
				gridRow = new SkillWeekGridRow(_rowManager, "NumericReadOnlyCell", "DailySmoothness", UserTexts.Resources.StandardDeviation);
				gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
				GridRows.Add(_rowManager.AddRow(gridRow));

				gridRow = new SkillWeekGridRow(_rowManager, "ReadOnlyPercentCell", "EstimatedServiceLevelShrinkage", UserTexts.Resources.ESL);
				gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
				GridRows.Add(_rowManager.AddRow(gridRow));
			}


			if (skill.SkillType.ForecastSource == ForecastSource.Email || skill.SkillType.ForecastSource == ForecastSource.Backoffice || skill.SkillType.ForecastSource == ForecastSource.Time)
			{
				gridRow = new SkillWeekGridRow(_rowManager, "TimeCell", "ForecastedHoursIncoming", UserTexts.Resources.ForecastedHoursIncoming);
				gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
				GridRows.Add(_rowManager.AddRow(gridRow));

				gridRow = new SkillWeekGridRow(_rowManager, "TimeCell", "ScheduledHoursIncoming", UserTexts.Resources.ScheduledHoursIncoming);
				gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
				GridRows.Add(_rowManager.AddRow(gridRow));

				gridRow = new SkillWeekGridRow(_rowManager, "TimeSpanCell", "AbsoluteIncomingDifference", UserTexts.Resources.AbsoluteDifferenceIncoming);
				gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
				GridRows.Add(_rowManager.AddRow(gridRow));

				gridRow = new SkillWeekGridRowStaffingIssues(_rowManager, "ReadOnlyPercentCell", "RelativeIncomingDifference", UserTexts.Resources.RelativeDifferenceIncoming, skill);
				gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
				GridRows.Add(_rowManager.AddRow(gridRow));
			}

            Rows.HeaderCount = 0;
        }
		
		public override void SetDataSource(ISchedulerStateHolder stateHolder, ISkill skill)
		{
			var skillWeekPeriods = _presenter.CreateDataSource(stateHolder, skill);
			if (skillWeekPeriods == null) return;
			_rowManager.SetDataSource(new List<IDictionary<DateOnlyPeriod, IList<ISkillStaffPeriod>>> { skillWeekPeriods });
		}

		public void SetupGrid(int colCount)
		{
			ColCount = colCount;
			RowCount = GridRows.Count - 1;
			ColWidths[0] = rowHeaderWidth;
		}

        public override void DrawDayGrid(ISchedulerStateHolder stateHolder,ISkill skill)
        {
           _presenter.DrawWeekGrid(stateHolder, skill);	
        }


        public override bool HasColumns
        {
			get { return _presenter.Weeks.Count > 0; }
        }
    }
}
