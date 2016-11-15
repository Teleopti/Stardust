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
		void Optimize(DateOnlyPeriod period, IEnumerable<IPerson> agentsToOptimize, IScheduleDictionary schedules, IEnumerable<ISkillDay> allSkillDays, IOptimizationPreferences optimizationPreferences);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public class MaxSeatOptimizationDoNothing : IMaxSeatOptimization
	{
		public void Optimize(DateOnlyPeriod period, IEnumerable<IPerson> agentsToOptimize, IScheduleDictionary schedules, IEnumerable<ISkillDay> allSkillDays, IOptimizationPreferences optimizationPreferences)
		{
		}
	}

	public class MaxSeatOptimization : IMaxSeatOptimization
	{
		private readonly MaxSeatSkillDataFactory _maxSeatSkillDataFactory;
		private readonly ResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
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

		public MaxSeatOptimization(MaxSeatSkillDataFactory maxSeatSkillDataFactory,
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
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
			LockDaysOnTeamBlockInfos lockDaysOnTeamBlockInfos)
		{
			_maxSeatSkillDataFactory = maxSeatSkillDataFactory;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
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
		}

		public void Optimize(DateOnlyPeriod period, IEnumerable<IPerson> agentsToOptimize, IScheduleDictionary schedules,
			IEnumerable<ISkillDay> allSkillDays, IOptimizationPreferences optimizationPreferences)
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
					var maxPeak = double.MinValue;
					var maxPeakDayAfter = _maxSeatPeak.Fetch(teamBlockInfo.BlockInfo.BlockPeriod.EndDate.AddDays(1), skillDaysForTeamBlockInfo);
					var maxPeakDayBefore = double.MinValue;
					var beforeAffected = dayUtcAffected(teamBlockInfo, datePoint, datePoint.AddDays(-1));
					var dayAffected = dayUtcAffected(teamBlockInfo, datePoint, datePoint);

					if (beforeAffected)
					{
						maxPeakDayBefore = _maxSeatPeak.Fetch(teamBlockInfo.BlockInfo.BlockPeriod.StartDate.AddDays(-1), skillDaysForTeamBlockInfo);
					}

					if (dayAffected)
					{
						maxPeak = _maxSeatPeak.Fetch(teamBlockInfo, skillDaysForTeamBlockInfo);
					}

					if (maxPeak.IsPositive() || maxPeakDayBefore.IsPositive())
					{
						var rollbackService = new SchedulePartModifyAndRollbackService(null, _scheduleDayChangeCallback, tagSetter);
						_teamBlockClearer.ClearTeamBlockWithNoResourceCalculation(rollbackService, teamBlockInfo, businessRules);
						var scheduleWasSuccess = _teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelectorForMaxSeat, teamBlockInfo,
							datePoint, schedulingOptions, rollbackService,
							new DoNothingResourceCalculateDelayer(), skillDaysForTeamBlockInfo.Union(allSkillDays), schedules,
							new ShiftNudgeDirective(), businessRules);

						if (!scheduleWasSuccess||
							!_restrictionOverLimitValidator.Validate(teamBlockInfo.MatrixesForGroupAndBlock(), optimizationPreferences) ||
							!_teamBlockShiftCategoryLimitationValidator.Validate(teamBlockInfo, null, optimizationPreferences) ||
							(_maxSeatPeak.Fetch(teamBlockInfo, skillDaysForTeamBlockInfo) >= maxPeak && dayAffected ) ||
							_maxSeatPeak.Fetch(teamBlockInfo.BlockInfo.BlockPeriod.EndDate.AddDays(1), skillDaysForTeamBlockInfo) > maxPeakDayAfter)
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
								var rollbackService = new SchedulePartModifyAndRollbackService(null, _scheduleDayChangeCallback, tagSetter);
								var scheduleDay = scheduleDayPro.DaySchedulePart();
								if (_isOverMaxSeat.Check(scheduleDay, skillDaysForTeamBlockInfo))
								{
									_deleteSchedulePartService.Delete(new List<IScheduleDay> {scheduleDay}, rollbackService, businessRules);
								}
							}
						}
					}
				}
			}
		}

		private bool dayUtcAffected(ITeamBlockInfo teamBlockInfo, DateOnly dateOnly, DateOnly affectDate)
		{
			foreach (var scheduleMatrixPro in teamBlockInfo.MatrixesForGroupAndBlock())
			{
				foreach (var scheduleDayPro in scheduleMatrixPro.UnlockedDays.Where(x => x.Day == dateOnly))
				{
					var scheduleDay = scheduleDayPro.DaySchedulePart();
					var startAffectDay = scheduleDay.PersonAssignment(true).ShiftLayers.Any(x => x.Period.StartDateTime.Date == affectDate.Date);
					if (startAffectDay)
						return true;
				}
			}

			return false;
		}
	}
}