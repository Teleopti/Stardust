using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class MaxSeatOptimization
	{
		private readonly MaxSeatSkillDataFactory _maxSeatSkillDataFactory;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly TeamBlockScheduler _teamBlockScheduler;
		private readonly TeamBlockClearer _teamBlockClearer;
		private readonly WorkShiftSelectorForMaxSeat _workShiftSelectorForMaxSeat;
		private readonly ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;
		private readonly RestrictionOverLimitValidator _restrictionOverLimitValidator;
		private readonly MaxSeatPeak _maxSeatPeak;
		private readonly IDeleteSchedulePartService _deleteSchedulePartService;
		private readonly IsOverMaxSeat _isOverMaxSeat;
		private readonly ScheduleChangesAffectedDates _scheduleChangesAffectedDates;
		private readonly ScheduledTeamBlockInfoFactory _scheduledTeamBlockInfoFactory;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;
		private readonly OptimizationLimitsForAgentFactory _optimizationLimitsForAgentFactory;
		private readonly SetMainShiftOptimizeActivitySpecificationForTeamBlock _setMainShiftOptimizeActivitySpecificationForTeamBlock;

		public MaxSeatOptimization(MaxSeatSkillDataFactory maxSeatSkillDataFactory,
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
			ISchedulingOptionsCreator schedulingOptionsCreator,
			TeamBlockScheduler teamBlockScheduler,
			TeamBlockClearer teamBlockClearer,
			WorkShiftSelectorForMaxSeat workShiftSelectorForMaxSeat,
			ITeamBlockShiftCategoryLimitationValidator teamBlockShiftCategoryLimitationValidator,
			RestrictionOverLimitValidator restrictionOverLimitValidator,
			MaxSeatPeak maxSeatPeak,
			IDeleteSchedulePartService deleteSchedulePartService,
			IsOverMaxSeat isOverMaxSeat,
			ScheduleChangesAffectedDates scheduleChangesAffectedDates,
			ScheduledTeamBlockInfoFactory scheduledTeamBlockInfoFactory,
			IGroupPersonSkillAggregator groupPersonSkillAggregator,
			OptimizationLimitsForAgentFactory optimizationLimitsForAgentFactory,
			SetMainShiftOptimizeActivitySpecificationForTeamBlock setMainShiftOptimizeActivitySpecificationForTeamBlock)
		{
			_maxSeatSkillDataFactory = maxSeatSkillDataFactory;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_teamBlockScheduler = teamBlockScheduler;
			_teamBlockClearer = teamBlockClearer;
			_workShiftSelectorForMaxSeat = workShiftSelectorForMaxSeat;
			_teamBlockShiftCategoryLimitationValidator = teamBlockShiftCategoryLimitationValidator;
			_restrictionOverLimitValidator = restrictionOverLimitValidator;
			_maxSeatPeak = maxSeatPeak;
			_deleteSchedulePartService = deleteSchedulePartService;
			_isOverMaxSeat = isOverMaxSeat;
			_scheduleChangesAffectedDates = scheduleChangesAffectedDates;
			_scheduledTeamBlockInfoFactory = scheduledTeamBlockInfoFactory;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
			_optimizationLimitsForAgentFactory = optimizationLimitsForAgentFactory;
			_setMainShiftOptimizeActivitySpecificationForTeamBlock = setMainShiftOptimizeActivitySpecificationForTeamBlock;
		}

		public void Optimize(ISchedulingProgress backgroundWorker, DateOnlyPeriod period, 
			IEnumerable<IPerson> agentsToOptimize, 
			IScheduleDictionary schedules, 
			IEnumerable<ISkillDay> allSkillDays, 
			IOptimizationPreferences optimizationPreferences, 
			IMaxSeatCallback maxSeatCallback)
		{
			if (optimizationPreferences.Advanced.UserOptionMaxSeatsFeature == MaxSeatsFeatureOptions.DoNotConsiderMaxSeats)
				return;
			var allAgents = schedules.Select(schedule => schedule.Key).ToArray();
			var maxSeatData = _maxSeatSkillDataFactory.Create(period, agentsToOptimize, schedules.Scenario, allAgents, allSkillDays.IntervalLengthInMinutes());
			if (!maxSeatData.MaxSeatSkillExists())
				return;
			var tagSetter = optimizationPreferences.General.CreateScheduleTagSetter();
			var businessRules = NewBusinessRuleCollection.Minimum();
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
			var teamBlockInfos = _scheduledTeamBlockInfoFactory.Create(period, agentsToOptimize, schedules, allAgents, schedulingOptions);
			var allSkillDaysExceptMaxSeat = allSkillDays.Except(x => x.Skill is MaxSeatSkill).ToArray();
			var optimizationLimits = _optimizationLimitsForAgentFactory.Create(optimizationPreferences, teamBlockInfos);

			using (_resourceCalculationContextFactory.Create(schedules, maxSeatData.AllMaxSeatSkills(), Enumerable.Empty<ExternalStaff>(), false, period.Inflate(1)))
			{
				var checkedInfos = 1;
				var numInfos = teamBlockInfos.Count();
				foreach (var teamBlockInfo in teamBlockInfos)
				{
					if (backgroundWorker.CancellationPending)
						continue;
					var datePoint = teamBlockInfo.BlockInfo.DatePoint(period);
					var skillDaysForTeamBlockInfo = maxSeatData.SkillDaysFor(teamBlockInfo, datePoint);
					var maxPeaksBefore = _maxSeatPeak.Fetch(teamBlockInfo, skillDaysForTeamBlockInfo);
					backgroundWorker.ReportProgress(0,
						new ResourceOptimizerProgressEventArgs(0, 0, Resources.OptimizingMaxSeats + " " + checkedInfos + "/" + numInfos,
							optimizationPreferences.Advanced.RefreshScreenInterval));
					if (maxPeaksBefore.HasPeaks())
					{
						var scheduleCallback = new ScheduleChangeCallbackForMaxSeatOptimization(_scheduleChangesAffectedDates);
						_setMainShiftOptimizeActivitySpecificationForTeamBlock.Execute(optimizationPreferences, teamBlockInfo, schedulingOptions);
						var rollbackService = new SchedulePartModifyAndRollbackService(null, scheduleCallback, tagSetter);
						_teamBlockClearer.ClearTeamBlockWithNoResourceCalculation(rollbackService, teamBlockInfo, businessRules);
						//TODO: should pass in orginal assignments here to fix same issue as #45540 for shiftswithinday
						var noResCalcData = new ResourceCalculationData(Enumerable.Empty<ISkill>(), new SkillResourceCalculationPeriodWrapper(new List<KeyValuePair<ISkill, IResourceCalculationPeriodDictionary>>()));
						var scheduleWasSuccess = _teamBlockScheduler.ScheduleTeamBlockDay(Enumerable.Empty<IPersonAssignment>(), new NoSchedulingCallback(), _workShiftSelectorForMaxSeat, teamBlockInfo,
							datePoint, schedulingOptions, rollbackService,
							new DoNothingResourceCalculateDelayer(), allSkillDaysExceptMaxSeat.Union(skillDaysForTeamBlockInfo).ToSkillSkillDayDictionary(), schedules, noResCalcData,
							new ShiftNudgeDirective(), businessRules, _groupPersonSkillAggregator);
						var maxPeaksAfter = _maxSeatPeak.Fetch(scheduleCallback.ModifiedDates, skillDaysForTeamBlockInfo);

						if (scheduleWasSuccess &&
								_restrictionOverLimitValidator.Validate(teamBlockInfo.MatrixesForGroupAndBlock(), optimizationPreferences) &&
								_teamBlockShiftCategoryLimitationValidator.Validate(teamBlockInfo, null, optimizationPreferences) &&
								maxPeaksAfter.IsBetterThan(maxPeaksBefore, scheduleCallback.ModifiedDates) && 
								!optimizationLimits.ForAgent(teamBlockInfo.TeamInfo.GroupMembers.First()).MoveMaxDaysOverLimit())
						{
							maxSeatCallback?.DatesOptimized(scheduleCallback.ModifiedDates);
						}
						else
						{
							rollbackService.RollbackMinimumChecks();
						}
					}

					checkedInfos++;
				}

				if (optimizationPreferences.Advanced.UserOptionMaxSeatsFeature != MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak)
					return;

				backgroundWorker.ReportProgress(0, new ResourceOptimizerProgressEventArgs(0, 0, Resources.DoNotBreakMaxSeatDotDotDot, optimizationPreferences.Advanced.RefreshScreenInterval));
				foreach (var teamBlockInfo in teamBlockInfos)
				{
					if (backgroundWorker.CancellationPending)
						return;
					var datePoint = teamBlockInfo.BlockInfo.DatePoint(period);
					var skillDaysForTeamBlockInfo = maxSeatData.SkillDaysFor(teamBlockInfo, datePoint);
					
					foreach (var date in period.DayCollection())
					{
						foreach (var scheduleMatrixPro in teamBlockInfo.MatrixesForGroupAndBlock())
						{
							foreach (var scheduleDayPro in scheduleMatrixPro.UnlockedDays.Where(x => x.Day == date))
							{
								var scheduleDay = scheduleDayPro.DaySchedulePart();
								if (!agentsToOptimize.Contains(scheduleDay.Person))
									continue;
								var scheduleCallback = new ScheduleChangeCallbackForMaxSeatOptimization(_scheduleChangesAffectedDates);
								if (_isOverMaxSeat.Check(scheduleDay, skillDaysForTeamBlockInfo))
								{
									_deleteSchedulePartService.Delete(new [] {scheduleDay}, new SchedulePartModifyAndRollbackService(null, scheduleCallback, tagSetter), businessRules);
									maxSeatCallback?.DatesOptimized(scheduleCallback.ModifiedDates);
								}
							}
						}
					}
				}
			}
		}
	}
}