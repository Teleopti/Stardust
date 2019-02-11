using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SkillResult
{

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class SkillDayGridControl : SkillResultGridControlBase
    {
		private readonly ISkillPriorityProvider _skillPriorityProvider;
		private const int rowHeaderWidth = 200;
        private const int cellWidth = 55;
        private const string settingName = "SchedulerSkillDayGridAndChart";
        private RowManagerScheduler<SkillDayGridRow, IDictionary<DateTime,IList<ISkillStaffPeriod>>> _rowManager;
        private static readonly Color ColorCells = Color.AntiqueWhite;
        private IList<DateOnly> _dates;

        //constructor
        public SkillDayGridControl(ISkillPriorityProvider skillPriorityProvider)
        {
	        _skillPriorityProvider = skillPriorityProvider;
	        initializeComponent();
            initializeGrid();
            InitializeBase(settingName);
        }

        //initialize component
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

        //initialize grid
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

        private GridCellModelBase initializeCallPercentReadOnlyCell()
        {
        	var cellModel = new PercentReadOnlyCellModel(Model) {NumberOfDecimals = 1};
        	return cellModel;
        }

        //event col width
        private void gridSkillDataQueryColWidth(object sender, GridRowColSizeEventArgs e)
        {
            if (e.Index <= Cols.HeaderCount)
            {
                e.Size = rowHeaderWidth;
            }

            if (e.Index > Cols.HeaderCount)
            {
                e.Size = cellWidth;
                e.Handled = true;
            }
        }

        private void gridSkillDataQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
        {
            if (e.ColIndex < 0 || e.RowIndex < 0) return;
			if (GridRows == null)
				return;
            if (e.ColIndex == 0 && e.RowIndex == 1)
                e.Style.CellValue = " ";

			if (e.RowIndex < GridRows.Count)
				GridRows[e.RowIndex].QueryCellInfo(GetCellInfo(e.Style, e.ColIndex, e.RowIndex));

            if (e.ColIndex > 0)
                e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;

            if (e.RowIndex == 1 && e.ColIndex > 0)
            {
                if(e.Style.Tag is DateOnly)
                    WeekendOrWeekday(e, (DateOnly)e.Style.Tag, false, true);
            }
            
            e.Handled = true;
        }

        private void createGridRows(ISkill skill,IList<DateOnly> dates, ISchedulerStateHolder schedulerStateHolder)
        {
            ((NumericReadOnlyCellModel)CellModels["NumericReadOnlyCell"]).NumberOfDecimals = 2;

			GridRows = new List<IGridRow>
                        	{
                        		new DateHeaderGridRow(DateHeaderType.WeekDates, dates),
                        		new DateHeaderGridRow(DateHeaderType.MonthDayNumber, dates)
                        	};

        	DateOnly baseDate = dates.Count > 0 ? dates.First() : DateOnly.MinValue;

        	_rowManager = new RowManagerScheduler<SkillDayGridRow, IDictionary<DateTime, IList<ISkillStaffPeriod>>>(
        		this, new List<IntervalDefinition>(), 15, schedulerStateHolder) {BaseDate = baseDate.Date};

        	SkillDayGridRow gridRow;
			if (skill.SkillType.ForecastSource == ForecastSource.MaxSeatSkill)
			{
				gridRow = new SkillDayGridRowMaxSeatsIssues(_rowManager, "NumericReadOnlyCell", "MaxUsedSeats", UserTexts.Resources.MaxUsedSeats);
				gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
				GridRows.Add(_rowManager.AddRow(gridRow));
			}

			if (skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill)
			{
				gridRow = new SkillDayGridRow(_rowManager, "TimeCell", "ForecastedHours", UserTexts.Resources.ForecastedHours);
				gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
				GridRows.Add(_rowManager.AddRow(gridRow));

                if(!skill.IsVirtual)
                {
                    gridRow = new SkillDayGridRowMinMaxIssues(_rowManager, "TimeCell", "ScheduledHours", UserTexts.Resources.ScheduledHours);
					gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
					GridRows.Add(_rowManager.AddRow(gridRow));
                }
                else
                {
					gridRow = new SkillDayGridRowMinMaxIssuesSummary(_rowManager, "TimeCell", "ScheduledHours",
					UserTexts.Resources.ScheduledHours);

					gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
					GridRows.Add(_rowManager.AddRow(gridRow));
                }

				gridRow = new SkillDayGridRow(_rowManager, "TimeSpanCell", "AbsoluteDifference", UserTexts.Resources.AbsoluteDifference);
				gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
				GridRows.Add(_rowManager.AddRow(gridRow));

                if (!skill.IsVirtual)
                {
                    gridRow = new SkillDayGridRowStaffingIssues(_rowManager, "ReadOnlyPercentCell", "RelativeDifference",
                                                                UserTexts.Resources.RelativeDifference, skill);
					gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
					GridRows.Add(_rowManager.AddRow(gridRow));
                }
                else
                {
                    gridRow = new SkillDayGridRowStaffingIssuesSummary(_rowManager, "ReadOnlyPercentCell", "RelativeDifference",
                                                                UserTexts.Resources.RelativeDifference);
					gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
					GridRows.Add(_rowManager.AddRow(gridRow));
                }

			    gridRow = new SkillDayGridRowBoostedRelativeDifference(_rowManager, "NumericReadOnlyCell", "RelativeBoostedDifference", UserTexts.Resources.AdjustedDifference, skill, _skillPriorityProvider);
				gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
				GridRows.Add(_rowManager.AddRow(gridRow));

				gridRow = new SkillDayGridRow(_rowManager, "NumericReadOnlyCell", "RootMeanSquare", UserTexts.Resources.RMS);
				gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
				GridRows.Add(_rowManager.AddRow(gridRow));

				gridRow = new SkillDayGridRow(_rowManager, "NumericReadOnlyCell", "DailySmoothness", UserTexts.Resources.StandardDeviation);
				gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
				GridRows.Add(_rowManager.AddRow(gridRow));

				if ((skill.SkillType.ForecastSource == ForecastSource.InboundTelephony || skill.SkillType.ForecastSource == ForecastSource.Chat) 
					&& !skill.IsVirtual )
				{
					gridRow = new SkillDayGridIntervalIssues(_rowManager, "ReadOnlyPercentCell", "HighestDeviationInPeriod", Resources.LowestIntraIntervalBalance);
					gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
					GridRows.Add(_rowManager.AddRow(gridRow));
				}

				gridRow = new SkillDayGridRow(_rowManager, "ReadOnlyPercentCell", "EstimatedServiceLevelShrinkage", UserTexts.Resources.ESL);
				gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
				GridRows.Add(_rowManager.AddRow(gridRow));
			}
           

            if (skill.SkillType.ForecastSource == ForecastSource.Email || skill.SkillType.ForecastSource == ForecastSource.Backoffice || skill.SkillType.ForecastSource == ForecastSource.Time)
            {
                gridRow = new SkillDayGridRow(_rowManager, "TimeCell", "ForecastedHoursIncoming", UserTexts.Resources.ForecastedHoursIncoming);
				gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
				GridRows.Add(_rowManager.AddRow(gridRow));

                gridRow = new SkillDayGridRow(_rowManager, "TimeCell", "ScheduledHoursIncoming", UserTexts.Resources.ScheduledHoursIncoming);
				gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
				GridRows.Add(_rowManager.AddRow(gridRow));

                gridRow = new SkillDayGridRow(_rowManager, "TimeSpanCell", "AbsoluteIncomingDifference", UserTexts.Resources.AbsoluteDifferenceIncoming);
				gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
				GridRows.Add(_rowManager.AddRow(gridRow));

                gridRow = new SkillDayGridRowStaffingIssues(_rowManager, "ReadOnlyPercentCell", "RelativeIncomingDifference", UserTexts.Resources.RelativeDifferenceIncoming, skill);
				gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
				GridRows.Add(_rowManager.AddRow(gridRow));
            }

            Rows.HeaderCount = 1;
        }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public override void SetDataSource(ISchedulerStateHolder stateHolder,ISkill skill)
		{
		    var stateHolderTimeZone = TimeZoneGuardForDesktop.Instance_DONTUSE.CurrentTimeZone();
            var dateTimePeriods = stateHolder.RequestedPeriod.Period(stateHolderTimeZone).WholeDayCollection(stateHolderTimeZone);
            _dates = dateTimePeriods.Select(d => new DateOnly(TimeZoneHelper.ConvertFromUtc(d.StartDateTime, stateHolderTimeZone))).ToList();
			var dataSource = createDataSourceDictionary(dateTimePeriods, stateHolder, skill);

			_rowManager.SetDataSource(new List<IDictionary<DateTime, IList<ISkillStaffPeriod>>> { dataSource });
		}

        public override void DrawDayGrid(ISchedulerStateHolder stateHolder,ISkill skill)
        {
           
            if (stateHolder == null || skill==null) return;

            using (PerformanceOutput.ForOperation("DrawSkillDayGrid"))
            {
                Model.Options.MergeCellsMode = GridMergeCellsMode.OnDemandCalculation |
                                               GridMergeCellsMode.MergeColumnsInRow;
				var timeZone = TimeZoneGuardForDesktop.Instance_DONTUSE.CurrentTimeZone();
                var dateTimePeriods = stateHolder.RequestedPeriod.Period(timeZone).WholeDayCollection(timeZone);
                _dates = dateTimePeriods.Select(d => new DateOnly(TimeZoneHelper.ConvertFromUtc(d.StartDateTime, timeZone))).ToList();
                createGridRows(skill, _dates, stateHolder);
            	SetDataSource(stateHolder, skill);
                ColCount = _dates.Count;
				RowCount = GridRows.Count - 1;
                ColWidths[0] = rowHeaderWidth;

                Model.MergeCells.DelayMergeCells(GridRangeInfo.Table());
            }
        }

        private static IDictionary<DateTime, IList<ISkillStaffPeriod>> createDataSourceDictionary(IList<DateTimePeriod> dateTimePeriods, ISchedulerStateHolder stateHolder, ISkill skill)
        {
            ISkillStaffPeriodDictionary skillStaffPeriods;
            IAggregateSkill aggregateSkillSkill = skill;
            if (!aggregateSkillSkill.IsVirtual)
            {
                if (
                    !stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.
                         TryGetValue(skill, out skillStaffPeriods))
                    skillStaffPeriods = new SkillStaffPeriodDictionary(skill);
            }
            else
            {
                skillStaffPeriods = new SkillStaffPeriodDictionary(skill);
                foreach (DateTimePeriod period in dateTimePeriods)
                {
                    ISkillStaffPeriodDictionary temp = stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(aggregateSkillSkill,
                                                                                                  period,
                                                                                                  true);
                    foreach (KeyValuePair<DateTimePeriod, ISkillStaffPeriod> keyValuePair in temp)
                    {
                        skillStaffPeriods.Add(keyValuePair);
                    }
                }
            }

            IDictionary<DateTime, IList<ISkillStaffPeriod>> returnDictionary = new Dictionary<DateTime, IList<ISkillStaffPeriod>>();
            var sortedSkillStaffPeriods = skillStaffPeriods.OrderBy(k => k.Key.StartDateTime).ToList();
            int count = sortedSkillStaffPeriods.Count;
            
            foreach (DateTimePeriod period in dateTimePeriods)
            {
                int currentIndex = 0;
                var skillStaffPeriodList = new List<ISkillStaffPeriod>();
                while (currentIndex<count)
                {
                    if (sortedSkillStaffPeriods[currentIndex].Key.Intersect(period))
                        skillStaffPeriodList.Add(sortedSkillStaffPeriods[currentIndex].Value);
                    currentIndex++;
                }
                returnDictionary.Add(period.StartDateTime,skillStaffPeriodList);
            }

            return returnDictionary;
        }

        public void WeekendOrWeekday(GridQueryCellInfoEventArgs e, DateOnly date, bool backColor, bool textColor)
        {
            if (DateHelper.IsWeekend(date, CultureInfo.CurrentCulture))
            {
                if (backColor)
                    e.Style.BackColor = ColorHolidayCell;
                if (textColor)
                    e.Style.TextColor = ColorHolidayHeader;
            }
            else
            {
                if(backColor)
                    e.Style.BackColor = ColorCells;
                   
                e.Style.TextColor = ForeColor;
            }
        }

        public override bool HasColumns
        {
            get { return _dates.Count > 0; }
        }

		public override int MainHeaderRow
		{
			get { return 1; }
		}
    }
}
