using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.Win.Common.Controls.Rows;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Chart;
using Teleopti.Ccc.WinCode.Common.Rows;
using Teleopti.Ccc.WinCode.Forecasting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling.SkillResult
{
    public class SkillIntradayGridPresenter
    {
        private readonly IList<IGridRow> _gridRows = new List<IGridRow>();
        private readonly TeleoptiGridControl _gridControl;
        private RowManagerScheduler<SkillStaffPeriodGridRowScheduler, ISkillStaffPeriod> _rowManager;
        private ISkill _skill;
        private IList<IntervalDefinition> _intervals = new List<IntervalDefinition>();
        private ISkillType _lastSkillType;
        private bool _isLastVirtual;
    	private bool _hasHelp = true;

        private  ChartSettings _chartSettings = new ChartSettings();
        private readonly ChartSettings _defaultChartSettings = new ChartSettings();

        private readonly IToggleManager _toggleManager;

        public SkillIntradayGridPresenter(TeleoptiGridControl gridControl, ChartSettings chartSettings)
        {
            _gridControl = gridControl;
            _chartSettings = chartSettings;
        }

        public SkillIntradayGridPresenter(TeleoptiGridControl gridControl, string settingName)
        {
            _gridControl = gridControl;
            setupChartDefault();

            //temp
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                _chartSettings = new PersonalSettingDataRepository(uow).FindValueByKey(settingName, _defaultChartSettings);
            }
        }

        public ReadOnlyCollection<IGridRow> GridRows
        {
            get{return new ReadOnlyCollection<IGridRow>(_gridRows);}
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
				_skill = skill;

				var period = createDateTimePeriod(skillStaffPeriods);
				createIntervalList(period, stateHolder);

				if (_gridRows.Count == 0 || !_skill.SkillType.Equals(_lastSkillType) || !_skill.IsVirtual.Equals(_isLastVirtual))
				{
					createGridRows(includeStatistics, skill.IsVirtual, stateHolder);
					_lastSkillType = _skill.SkillType;
					_isLastVirtual = _skill.IsVirtual;
				}
				else
				{
					//1. Replace the headers
					_gridRows[0] = new IntervalHeaderGridRow(_intervals);
					//2. Fix the rowmanager intervals and resolution
					_rowManager.Intervals = _intervals;
					_rowManager.IntervalLength = _skill.DefaultResolution;
				}

				_rowManager.BaseDate = TimeZoneHelper.ConvertToUtc(period.StartDateTimeLocal(stateHolder.TimeZoneInfo).Date,
				                                                   stateHolder.TimeZoneInfo);
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
            _rowManager = new RowManagerScheduler<SkillStaffPeriodGridRowScheduler, ISkillStaffPeriod>(_gridControl, _intervals, _skill.DefaultResolution, stateHolder);
            _gridControl.CoveredRanges.Clear();
        	SkillStaffPeriodGridRowScheduler gridRow;

			if (_skill.SkillType.ForecastSource == ForecastSource.MaxSeatSkill)
			{
				gridRow = new SkillStaffPeriodGridRowSchedulerMaxSeatIssues(_rowManager, "NumericReadOnlyCell", "Payload.CalculatedUsedSeats", UserTexts.Resources.UsedSeats);
				gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
				_gridRows.Add(_rowManager.AddRow(gridRow));

				gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "IntegerReadOnlyCell", "Payload.MaxSeats", UserTexts.Resources.MaxSeats);
				gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
				_gridRows.Add(_rowManager.AddRow(gridRow));
			}

			TextManager manager = new TextManager(_skill.SkillType);
			if (_skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill)
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
                                                                                 UserTexts.Resources.RelativeDifference,
                                                                                 _skill);
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
                                                                                UserTexts.Resources.AdjustedDifference, _skill);
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
				

				gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "NumericReadOnlyCell", "IntraIntervalDeviation",
				                                               UserTexts.Resources.IntervalStdev);
				gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
				_gridRows.Add(_rowManager.AddRow(gridRow));



				if (_skill.SkillType.ForecastSource == ForecastSource.Email ||
				    _skill.SkillType.ForecastSource == ForecastSource.Backoffice ||
				    _skill.SkillType.ForecastSource == ForecastSource.Time)
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
					                                                             UserTexts.Resources.RelativeDifferenceIncoming, _skill);
					gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
					_gridRows.Add(_rowManager.AddRow(gridRow));

				}

                //feature flag here
                var estimatedServiceLevelPropertyName = "EstimatedServiceLevel";
			    if (_toggleManager.IsEnabled(Toggles.Scheduler_ShowIntadayESLWithShrinkage_21874))
			        estimatedServiceLevelPropertyName = "EstimatedServiceLevelShrinkage";

				gridRow = new SkillStaffPeriodGridRowScheduler(_rowManager, "PercenFromPercentReadOnlyCellModel",
				                                               estimatedServiceLevelPropertyName, UserTexts.Resources.ESL);
				gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
				_gridRows.Add(_rowManager.AddRow(gridRow));
			}

        	if (includeStatistics)
            {
                if (_skill.SkillType.ForecastSource == ForecastSource.InboundTelephony)
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
                new PersonalSettingDataRepository(uow).PersistSettingValue(_chartSettings);
                uow.PersistAll();
            }
        }

        private void createIntervalList(DateTimePeriod period, ISchedulerStateHolder stateHolder)
        {
            _intervals.Clear();
            _intervals = period.IntervalsFromHourCollection(_skill.DefaultResolution, stateHolder.TimeZoneInfo);
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
