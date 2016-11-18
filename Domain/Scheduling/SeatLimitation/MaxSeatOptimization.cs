using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public interface IMaxSeatOptimization
	{
		void Optimize(DateOnlyPeriod period, IEnumerable<IPerson> agentsToOptimize, IScheduleDictionary schedules, IEnumerable<ISkillDay> allSkillDays, IOptimizationPreferences optimizationPreferences, IMaxSeatCallback maxSeatCallback);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public class MaxSeatOptimizationDoNothing : IMaxSeatOptimization
	{
		public void Optimize(DateOnlyPeriod period, IEnumerable<IPerson> agentsToOptimize, IScheduleDictionary schedules, IEnumerable<ISkillDay> allSkillDays, IOptimizationPreferences optimizationPreferences, IMaxSeatCallback maxSeatCallback)
		{
		}
	}

	public class MaxSeatOptimization : IMaxSeatOptimization
	{
		private readonly MaxSeatSkillDataFactory _maxSeatSkillDataFactory;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly ITeamBlockGenerator _teamBlockGenerator;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly WorkShiftSelectorForMaxSeat _workShiftSelectorForMaxSeat;
		private readonly ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;
		private readonly RestrictionOverLimitValidator _restrictionOverLimitValidator;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly MaxSeatPeak _maxSeatPeak;
		private readonly TeamInfoFactoryFactory _teamInfoFactoryFactory;
		private readonly IDeleteSchedulePartService _deleteSchedulePartService;
		private readonly IsOverMaxSeat _isOverMaxSeat;
		private readonly LockDaysOnTeamBlockInfos _lockDaysOnTeamBlockInfos;
		private readonly ScheduleChangesAffectedDates _scheduleChangesAffectedDates;

		public MaxSeatOptimization(MaxSeatSkillDataFactory maxSeatSkillDataFactory,
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
			ISchedulingOptionsCreator schedulingOptionsCreator,
			ITeamBlockScheduler teamBlockScheduler,
			ITeamBlockGenerator teamBlockGenerator,
			ITeamBlockClearer teamBlockClearer,
			WorkShiftSelectorForMaxSeat workShiftSelectorForMaxSeat,
			ITeamBlockShiftCategoryLimitationValidator teamBlockShiftCategoryLimitationValidator,
			RestrictionOverLimitValidator restrictionOverLimitValidator,
			IMatrixListFactory matrixListFactory,
			MaxSeatPeak maxSeatPeak,
			TeamInfoFactoryFactory teamInfoFactoryFactory,
			IDeleteSchedulePartService deleteSchedulePartService,
			IsOverMaxSeat isOverMaxSeat,
			LockDaysOnTeamBlockInfos lockDaysOnTeamBlockInfos,
			ScheduleChangesAffectedDates scheduleChangesAffectedDates)
		{
			_maxSeatSkillDataFactory = maxSeatSkillDataFactory;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_teamBlockScheduler = teamBlockScheduler;
			_teamBlockGenerator = teamBlockGenerator;
			_teamBlockClearer = teamBlockClearer;
			_workShiftSelectorForMaxSeat = workShiftSelectorForMaxSeat;
			_teamBlockShiftCategoryLimitationValidator = teamBlockShiftCategoryLimitationValidator;
			_restrictionOverLimitValidator = restrictionOverLimitValidator;
			_matrixListFactory = matrixListFactory;
			_maxSeatPeak = maxSeatPeak;
			_teamInfoFactoryFactory = teamInfoFactoryFactory;
			_deleteSchedulePartService = deleteSchedulePartService;
			_isOverMaxSeat = isOverMaxSeat;
			_lockDaysOnTeamBlockInfos = lockDaysOnTeamBlockInfos;
			_scheduleChangesAffectedDates = scheduleChangesAffectedDates;
		}

		public void Optimize(DateOnlyPeriod period, 
			IEnumerable<IPerson> agentsToOptimize, 
			IScheduleDictionary schedules, 
			IEnumerable<ISkillDay> allSkillDays, 
			IOptimizationPreferences optimizationPreferences, 
			IMaxSeatCallback maxSeatCallback)
		{
			if (optimizationPreferences.Advanced.UserOptionMaxSeatsFeature == MaxSeatsFeatureOptions.DoNotConsiderMaxSeats)
				return;
			var allAgents = schedules.Select(schedule => schedule.Key).ToArray();
			var maxSeatData = _maxSeatSkillDataFactory.Create(period, agentsToOptimize, schedules.Scenario, allAgents,
				allSkillDays.Any() ? allSkillDays.Min(s => s.Skill.DefaultResolution) : 15);
			if (!maxSeatData.MaxSeatSkillExists())
				return;
			var tagSetter = optimizationPreferences.General.ScheduleTag == null
				? new ScheduleTagSetter(NullScheduleTag.Instance)
				: new ScheduleTagSetter(optimizationPreferences.General.ScheduleTag);
			var allMatrixes = _matrixListFactory.CreateMatrixListAllForLoadedPeriod(schedules, allAgents, period);
			var businessRules = NewBusinessRuleCollection.Minimum();
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
			_teamInfoFactoryFactory.Create(allAgents, schedules, optimizationPreferences.Extra.TeamGroupPage);
			var teamBlockInfos = _teamBlockGenerator.Generate(allAgents, allMatrixes, period, agentsToOptimize, schedulingOptions);

			_lockDaysOnTeamBlockInfos.LockUnscheduleDaysAndRemoveEmptyTeamBlockInfos(teamBlockInfos);

			using (_resourceCalculationContextFactory.Create(schedules, maxSeatData.AllMaxSeatSkills()))
			{
				foreach (var teamBlockInfo in teamBlockInfos)
				{
					var datePoint = teamBlockInfo.BlockInfo.DatePoint(period);
					var skillDaysForTeamBlockInfo = maxSeatData.SkillDaysFor(teamBlockInfo, datePoint);
					var maxPeaksBefore = _maxSeatPeak.Fetch(teamBlockInfo, skillDaysForTeamBlockInfo);
					if (maxPeaksBefore.HasPeaks())
					{
						var scheduleCallback = new ScheduleChangeCallbackForMaxSeatOptimization(_scheduleChangesAffectedDates);
						var rollbackService = new SchedulePartModifyAndRollbackService(null, scheduleCallback, tagSetter);
						_teamBlockClearer.ClearTeamBlockWithNoResourceCalculation(rollbackService, teamBlockInfo, businessRules);
						var scheduleWasSuccess = _teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelectorForMaxSeat, teamBlockInfo,
							datePoint, schedulingOptions, rollbackService,
							new DoNothingResourceCalculateDelayer(), skillDaysForTeamBlockInfo.Union(allSkillDays), schedules,
							new ShiftNudgeDirective(), businessRules);
						var maxPeaksAfter = _maxSeatPeak.Fetch(scheduleCallback.ModifiedDates, skillDaysForTeamBlockInfo);

						if (scheduleWasSuccess &&
								_restrictionOverLimitValidator.Validate(teamBlockInfo.MatrixesForGroupAndBlock(), optimizationPreferences) &&
								_teamBlockShiftCategoryLimitationValidator.Validate(teamBlockInfo, null, optimizationPreferences) &&
								maxPeaksAfter.IsBetterThan(maxPeaksBefore, scheduleCallback.ModifiedDates))
						{
							maxSeatCallback?.DatesOptimized(scheduleCallback.ModifiedDates);
						}
						else
						{
							rollbackService.RollbackMinimumChecks();
						}
					}
				}

				if (optimizationPreferences.Advanced.UserOptionMaxSeatsFeature != MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak)
					return;

				foreach (var teamBlockInfo in teamBlockInfos)
				{
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