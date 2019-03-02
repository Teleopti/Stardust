using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Chart;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common.Chart;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SkillResult
{
    public class SkillIntradayGridPresenter
    {
        private readonly IList<IGridRow> _gridRows = new List<IGridRow>();
        private readonly TeleoptiGridControl _gridControl;
        private RowManagerScheduler<SkillStaffPeriodGridRowScheduler, ISkillStaffPeriod> _rowManager;
	    public ISkill Skill { get; private set; }
	    private IList<IntervalDefinition> _intervals = new List<IntervalDefinition>();
        private ISkillType _lastSkillType;
        private bool _isLastVirtual;
    	private bool _hasHelp = true;

        private  ChartSettings _chartSettings = new ChartSettings();
	    private readonly ISkillPriorityProvider _skillPriorityProvider;
		private readonly ITimeZoneGuard _timeZoneGuard;
		private readonly ChartSettings _defaultChartSettings = new ChartSettings();


        public SkillIntradayGridPresenter(TeleoptiGridControl gridControl, ChartSettings chartSettings, ISkillPriorityProvider skillPriorityProvider, ITimeZoneGuard timeZoneGuard)
        {
            _gridControl = gridControl;
            _chartSettings = chartSettings;
	        _skillPriorityProvider = skillPriorityProvider;
			_timeZoneGuard = timeZoneGuard;
		}

        public SkillIntradayGridPresenter(TeleoptiGridControl gridControl, string settingName, ISkillPriorityProvider skillPriorityProvider, ITimeZoneGuard timeZoneGuard)
        {
            _gridControl = gridControl;
	        _skillPriorityProvider = skillPriorityProvider;
	        setupChartDefault();
			_timeZoneGuard = timeZoneGuard;

			//temp
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                _chartSettings = PersonalSettingDataRepository.DONT_USE_CTOR(uow).FindValueByKey(settingName, _defaultChartSettings);
            }
        }

        public ReadOnlyCollection<IGridRow> GridRows
        {
            get{return new ReadOnlyCollection<IGridRow>(_gridRows);}
        }

	    public TimeSpan? SelectedIntervalTime()
	    {
		    var x = _gridRows[0] as IntervalHeaderGridRow;
		    return x?.Time(new CellInfo {ColHeaderCount = 2, ColIndex = _gridControl.CurrentCellInfo?.ColIndex-1 ?? 1});
	    }

        public ReadOnlyCollection<IntervalDefinition> Intervals
        {
            get{return new ReadOnlyCollection<IntervalDefinition>(_intervals);}
        }

        public IDictionary<int, GridRow> EnabledChartGridRows
        {
            get
            {
                IDictionary<int, GridRow> settings = (from r in _gridRows.OfType<GridRow>()
                                                      where r.ChartSeriesSettings != null &&
                                                            r.ChartSeriesSettings.Enabled
                                                      select r).ToDictionary(k => _gridRows.IndexOf(k), v => v);

                return settings;
            }
        }

    	public bool HasHelp
    	{
			get { return _hasHelp;}
			set { _hasHelp = value; }
    	}

        public ChartSettings ChartSettings
        {
            get { return _chartSettings; }
        }

		public RowManagerScheduler<SkillStaffPeriodGridRowScheduler, ISkillStaffPeriod> RowManager
		{
			get {return _rowManager;}
		}

        public void SetDataSource(IList<ISkillStaffPeriod> skillStaffPeriods, ISkill skill, bool includeStatistics, ISchedulerStateHolder stateHolder)
        {
            if (skill == null) return;
			if (_gridControl.InvokeRequired)
			{
				_gridControl.BeginInvoke(new MethodInvoker(() => SetDataSource(skillStaffPeriods, skill, includeStatistics, stateHolder)));
			}
			else
			{
				_gridControl.Cols.DefaultSize = 55;
				Skill = skill;

				var period = createDateTimePeriod(skillStaffPeriods);
				createIntervalList(period);

				if (_gridRows.Count == 0 || !Skill.SkillType.Equals(_lastSkillType) || !Skill.IsVirtual.Equals(_isLastVirtual))
				{
					createGridRows(includeStatistics, skill.IsVirtual, stateHolder);
					_lastSkillType = Skill.SkillType;
					_isLastVirtual = Skill.IsVirtual;
				}
				else
				{
					//1. Replace the headers
					_gridRows[0] = new IntervalHeaderGridRow(_intervals);
					//2. Fix the rowmanager intervals and resolution
					_rowManager.Intervals = _intervals;
					_rowManager.IntervalLength = Skill.DefaultResolution;
				}

				_rowManager.BaseDate = TimeZoneHelper.ConvertToUtc(period.StartDateTimeLocal(_timeZoneGuard.CurrentTimeZone()).Date,
					_timeZoneGuard.CurrentTimeZone());
				_rowManager.SetDataSource(skillStaffPeriods);
			}
        }

        private IChartSeriesSetting configureSetting(string key)
        {
            IChartSeriesSetting ret = _chartSettings.DefinedSetting(key, new ChartSettingsManager().ChartSettingsDefault);
            ret.Enabled = _chartSettings.SelectedRows.Contains(key);
            return ret;
        }

        private void createGridRows(bool includeStatistics, bool isVirtual, ISchedulerStateHolder stateHolder)
        {
            ((NumericReadOnlyCellModel) _gridControl.CellModels["NumericReadOnlyCell"]).NumberOfDecimals = 2;
			((PercentReadOnlyCellModel)_gridControl.CellModels["PercentReadOnlyCell"]).NumberOfDecimals = 1;

            _gridRows.Clear();
            _gridRows.Add(new IntervalHeaderGridRow(_intervals));
            _rowManager = new RowManagerScheduler<SkillStaffPeriodGridRowScheduler, ISkillStaffPeriod>(_gridControl, _intervals, Skill.DefaultResolution, stateHolder);
            _gridControl.CoveredRanges.Clear();
        	SkillStaffPeriodGridRowScheduler gridRow;

			if (Skill.SkillType.ForecastSource == ForecastSource.MaxSeatSkill)
			{
				gridRow = new SkillStaffPeriodGridRowSchedulerMaxSeatIssues(_rowManager, "NumericReadOnlyCell", "Payload.CalculatedUsedSeats", UserTexts.Resources.UsedSeats);
				gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
				_gridRows.Add(_rowManager.AddRow(gridRow));

				gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "IntegerReadOnlyCell", "Payload.MaxSeats", UserTexts.Resources.MaxSeats);
				gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
				_gridRows.Add(_rowManager.AddRow(gridRow));
			}

			TextManager manager = new TextManager(Skill.SkillType);
			if (Skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill)
			{
				gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "NumericReadOnlyCell", "FStaff",
				                                               manager.WordDictionary["ForecastedAgents"]);
				gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
				_gridRows.Add(_rowManager.AddRow(gridRow));

                if(!isVirtual)
                {
                    gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "NumericReadOnlyCell", "CalculatedResource",
                                                               manager.WordDictionary["ScheduledStaff"]);
                    gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
                    _gridRows.Add(_rowManager.AddRow(gridRow));
                }
                else
                {
                    gridRow = new SkillStaffPeriodGridRowSchedulerMinMaxIssuesSummary(_rowManager, "NumericReadOnlyCell",
                                                                               "CalculatedResource",
                                                                               manager.WordDictionary["ScheduledStaff"]);
                    gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
                    _gridRows.Add(_rowManager.AddRow(gridRow));
                }
				

				if (!isVirtual)
				{
					gridRow = new SkillStaffPeriodGridRowSchedulerMinMaxIssues(_rowManager, "NumericReadOnlyCell",
					                                                           "CalculatedLoggedOn",
					                                                           UserTexts.Resources.ScheduledHeads);
					gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
					_gridRows.Add(_rowManager.AddRow(gridRow));
				}

				gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "NumericReadOnlyCell", "AbsoluteDifference",
				                                               UserTexts.Resources.AbsoluteDifference);
				gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
				_gridRows.Add(_rowManager.AddRow(gridRow));

                if (!isVirtual)
                {
                    gridRow = new SkillStaffPeriodGridRowSchedulerStaffingIssues(_rowManager, "PercentReadOnlyCell",
                                                                                 "RelativeDifferenceForDisplayOnly",
                                                                                 UserTexts.Resources.RelativeDifference);
                    gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
                    _gridRows.Add(_rowManager.AddRow(gridRow));
                }
                else
                {
                    gridRow = new SkillStaffPeriodGridRowSchedulerStaffingIssuesSummary(_rowManager, "PercentReadOnlyCell",
                                                                                 "RelativeDifferenceForDisplayOnly",
                                                                                 UserTexts.Resources.RelativeDifference);
                    gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
                    _gridRows.Add(_rowManager.AddRow(gridRow));
                }


                gridRow = new SkillStaffPeriodGridRowBoostedRelativeDifference(_rowManager, "NumericReadOnlyCell",
                                                                                "RelativeBoostedDifferenceForDisplayOnly",
                                                                                UserTexts.Resources.AdjustedDifference, Skill, _skillPriorityProvider);
                gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
                _gridRows.Add(_rowManager.AddRow(gridRow));


			    if (!isVirtual)
                {
                    gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "IntegerReadOnlyCell",
                                                               "Payload.SkillPersonData.MinimumPersons",
                                                               manager.WordDictionary["MinimumAgents"]);
                    gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
                    _gridRows.Add(_rowManager.AddRow(gridRow));

                    gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "IntegerReadOnlyCell",
                                                                   "Payload.SkillPersonData.MaximumPersons",
                                                                   manager.WordDictionary["MaximumAgents"]);
                    gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
                    _gridRows.Add(_rowManager.AddRow(gridRow));
                }
				if ((Skill.SkillType.ForecastSource == ForecastSource.InboundTelephony || Skill.SkillType.ForecastSource == ForecastSource.Chat) 
					&& !isVirtual)
				{
					gridRow = new SkillStaffPeriodGridRowIntraIntervalIssues(_rowManager, "PercentReadOnlyCell", "IntraIntervalValue", Resources.IntraIntervalOptimization);
					gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
					_gridRows.Add(_rowManager.AddRow(gridRow));
				}

				if (Skill.SkillType.ForecastSource == ForecastSource.Email ||
				    Skill.SkillType.ForecastSource == ForecastSource.Backoffice ||
				    Skill.SkillType.ForecastSource == ForecastSource.Time ||
					Skill.SkillType.ForecastSource == ForecastSource.OutboundTelephony)
				{

					gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "NumericReadOnlyCell",
					                                               "Payload.ForecastedIncomingDemand",
					                                               manager.WordDictionary["ForecastedAgentsIncoming"]);
					gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
					_gridRows.Add(_rowManager.AddRow(gridRow));

					gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "NumericReadOnlyCell", "ScheduledAgentsIncoming",
					                                               manager.WordDictionary["ScheduledAgentsIncoming"]);
					gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
					_gridRows.Add(_rowManager.AddRow(gridRow));

					gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "NumericReadOnlyCell", "IncomingDifference",
					                                               UserTexts.Resources.AbsoluteDifferenceIncoming);
					gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
					_gridRows.Add(_rowManager.AddRow(gridRow));

					gridRow = new SkillStaffPeriodGridRowSchedulerStaffingIssues(_rowManager, "PercentReadOnlyCell",
					                                                             "RelativeDifferenceIncoming",
					                                                             UserTexts.Resources.RelativeDifferenceIncoming);
					gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
					_gridRows.Add(_rowManager.AddRow(gridRow));

				}

				gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "PercenFromPercentReadOnlyCellModel",
															   "EstimatedServiceLevelShrinkage", UserTexts.Resources.ESL);
				gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
				_gridRows.Add(_rowManager.AddRow(gridRow));
			}

        	if (includeStatistics)
            {
                if (Skill.SkillType.ForecastSource == ForecastSource.InboundTelephony)
                {
                    gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "NumericReadOnlyCell",
                                                                    "Payload.TaskData.Tasks",
                                                                    UserTexts.Resources.ForecastedCalls);
                    gridRow.ChartSeriesSettings = configureSetting("TotalTasks");
                    _gridRows.Add(_rowManager.AddRow(gridRow));

                    gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "TimeSpanTotalSecondsReadOnlyCell",
                                                                    "Payload.TaskData.AverageHandlingTaskTime",
                                                                    UserTexts.Resources.ForecastedHandlingTime);
                    gridRow.ChartSeriesSettings = configureSetting("AverageHandlingTaskTime");
                    _gridRows.Add(_rowManager.AddRow(gridRow));

                    gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "TimeSpanTotalSecondsReadOnlyCell",
                                                                    "Payload.TaskData.AverageTaskTime",
                                                                    UserTexts.Resources.ForecastedTalkTime);
                    gridRow.ChartSeriesSettings = configureSetting("TotalAverageTaskTime");
                    _gridRows.Add(_rowManager.AddRow(gridRow));

                    gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "TimeSpanTotalSecondsReadOnlyCell",
                                                                    "Payload.TaskData.AverageAfterTaskTime",
                                                                    UserTexts.Resources.ForecastedAverageAfterCallWork);
                    gridRow.ChartSeriesSettings = configureSetting("AverageAfterTaskTime");
                    _gridRows.Add(_rowManager.AddRow(gridRow));

                    gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "IntegerReadOnlyCell",
                                                                    "ActiveAgentCount.ActiveAgents", UserTexts.Resources.ActualHeads);
                    gridRow.ChartSeriesSettings = configureSetting("ActiveAgents");
                    _gridRows.Add(_rowManager.AddRow(gridRow));

                    gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "NumericReadOnlyCell",
                                                                    "StatisticTask.StatAnsweredTasks", UserTexts.Resources.AnsweredCalls);
                    gridRow.ChartSeriesSettings = configureSetting("TotalStatisticAnsweredTasks");
                    _gridRows.Add(_rowManager.AddRow(gridRow));

                    gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "NumericReadOnlyCell",
                                                                    "StatisticTask.StatAbandonedTasks", UserTexts.Resources.AbandonedCalls);
                    gridRow.ChartSeriesSettings = configureSetting("TotalStatisticAbandonedTasks");
                    _gridRows.Add(_rowManager.AddRow(gridRow));

                    gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "NumericReadOnlyCell",
                                                                    "StatisticTask.StatAverageQueueTimeSeconds",
                                                                    UserTexts.Resources.AverageSpeedOfAnswer);
                    gridRow.ChartSeriesSettings = configureSetting("StatAverageQueueTimeSeconds");
                    _gridRows.Add(_rowManager.AddRow(gridRow));

                    gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "NumericReadOnlyCell",
                                                                    "StatisticTask.StatCalculatedTasks", UserTexts.Resources.CalculatedCalls);
                    gridRow.ChartSeriesSettings = configureSetting("TotalStatisticCalculatedTasks");
                    _gridRows.Add(_rowManager.AddRow(gridRow));

                    gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "NumericReadOnlyCell",
                                                                   "StatisticTask.StatAverageHandleTimeSeconds",
                                                                   UserTexts.Resources.ActualHandlingTime);
                    gridRow.ChartSeriesSettings = configureSetting("TotalStatisticAverageHandleTime");
                    _gridRows.Add(_rowManager.AddRow(gridRow));

                    gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "NumericReadOnlyCell",
                                                                    "StatisticTask.StatAverageTaskTimeSeconds",
                                                                    UserTexts.Resources.ActualTalkTime);
                    gridRow.ChartSeriesSettings = configureSetting("TotalStatisticAverageTaskTime");
                    _gridRows.Add(_rowManager.AddRow(gridRow));

                    gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "NumericReadOnlyCell",
                                                                   "StatisticTask.StatAverageAfterTaskTimeSeconds",
                                                                   UserTexts.Resources.ActualAfterCallWorkTime);
                    gridRow.ChartSeriesSettings = configureSetting("TotalStatisticAverageAfterTaskTime");
                    _gridRows.Add(_rowManager.AddRow(gridRow));


                }
                else
                {
                    gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "NumericReadOnlyCell",
                                                                    "Payload.TaskData.Tasks",
                                                                    manager.WordDictionary["ForecastedTasks"]);
                    gridRow.ChartSeriesSettings = configureSetting("TotalTasks");
                    _gridRows.Add(_rowManager.AddRow(gridRow));

                    gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "TimeSpanReadOnlyCell",
                                                                    "Payload.TaskData.AverageTaskTime",
                                                                    manager.WordDictionary["ForecastedHandlingTime"]);
                    gridRow.ChartSeriesSettings = configureSetting("TotalAverageTaskTime");
                    _gridRows.Add(_rowManager.AddRow(gridRow));

                    gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "IntegerReadOnlyCell",
																	"ActiveAgentCount.ActiveAgents", manager.WordDictionary["ActualAgents"]);
                    gridRow.ChartSeriesSettings = configureSetting("ActiveAgents");
                    _gridRows.Add(_rowManager.AddRow(gridRow));

                    gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "NumericReadOnlyCell",
                                                                    "StatisticTask.StatAnsweredTasks", manager.WordDictionary["TotalStatisticAnsweredTasks"]);
                    gridRow.ChartSeriesSettings = configureSetting("TotalStatisticAnsweredTasks");
                    _gridRows.Add(_rowManager.AddRow(gridRow));

                    gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "NumericReadOnlyCell",
                                                                    "StatisticTask.StatAbandonedTasks", manager.WordDictionary["TotalStatisticAbandonedTasks"]);
                    gridRow.ChartSeriesSettings = configureSetting("TotalStatisticAbandonedTasks");
                    _gridRows.Add(_rowManager.AddRow(gridRow));

                    gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "NumericReadOnlyCell",
                                                                    "StatisticTask.StatAverageQueueTimeSeconds",
                                                                    UserTexts.Resources.AverageSpeedOfAnswer);
                    gridRow.ChartSeriesSettings = configureSetting("StatAverageQueueTimeSeconds");
                    _gridRows.Add(_rowManager.AddRow(gridRow));

                    gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "NumericReadOnlyCell",
                                                                    "StatisticTask.StatCalculatedTasks", manager.WordDictionary["TotalStatisticCalculatedTasks"]);
                    gridRow.ChartSeriesSettings = configureSetting("TotalStatisticCalculatedTasks");
                    _gridRows.Add(_rowManager.AddRow(gridRow));

                    gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "NumericReadOnlyCell",
                                                                    "StatisticTask.StatAverageTaskTimeSeconds",
                                                                    manager.WordDictionary["ActualHandlingTime"]);
                    gridRow.ChartSeriesSettings = configureSetting("TotalStatisticAverageTaskTime");
                    _gridRows.Add(_rowManager.AddRow(gridRow));
                }

                gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "PercenFromPercentReadOnlyCellModel",
                                                                "ActualServiceLevel", UserTexts.Resources.ActualServiceLevel);
                gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
                _gridRows.Add(_rowManager.AddRow(gridRow));
            }
        }

        private void setupChartDefault()
        {
            _defaultChartSettings.SelectedRows.Add("FStaff");
            _defaultChartSettings.SelectedRows.Add("CalculatedResource");
            _defaultChartSettings.SelectedRows.Add("CalculatedLoggedOn");
        }

        public void SetRowVisibility(string key, bool enabled)
        {
            if (enabled)
            {
                if (!_chartSettings.SelectedRows.Contains(key))
                    _chartSettings.SelectedRows.Add(key);
            }
            else
            {
                _chartSettings.SelectedRows.Remove(key);
            }
        }

        public void ReloadChartSettings(ChartSettings chartSettings)
        {
            _chartSettings = chartSettings;
            foreach (GridRow gridRow in _gridRows.OfType<GridRow>())
            {
                if (gridRow.ChartSeriesSettings != null)
                    gridRow.ChartSeriesSettings = configureSetting(gridRow.ChartSeriesSettings.DisplayKey);
            }
        }

        public void SaveChartSettings()
        {
            // temporary solution to fix interaction problems between scheduler and intraday
            if (((ISettingValue)_chartSettings).BelongsTo == null)
                return;

            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                PersonalSettingDataRepository.DONT_USE_CTOR(uow).PersistSettingValue(_chartSettings);
                uow.PersistAll();
            }
        }

        private void createIntervalList(DateTimePeriod period)
        {
            _intervals.Clear();
            _intervals = period.IntervalsFromHourCollection(Skill.DefaultResolution, _timeZoneGuard.CurrentTimeZone());
        }

        private static DateTimePeriod createDateTimePeriod(ICollection<ISkillStaffPeriod> list)
        {
            if (list.Count == 0) return new DateTimePeriod();
            ISkillStaffPeriod first = list.First();
            ISkillStaffPeriod last = list.Last();
            return new DateTimePeriod(first.Period.StartDateTime, last.Period.EndDateTime);
        }
    }
}
