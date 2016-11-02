using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class MaxSeatOptimization
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
														TeamInfoFactoryFactory teamInfoFactoryFactory)
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
		}

		public void Optimize(DateOnlyPeriod period, IEnumerable<IPerson> agentsToOptimize, IScheduleDictionary schedules, IScenario scenario, IOptimizationPreferences optimizationPreferences, int maxSeatIntervalLength)
		{
			if (optimizationPreferences.Advanced.UserOptionMaxSeatsFeature.Equals(MaxSeatsFeatureOptions.DoNotConsiderMaxSeats))
				return;
			var allAgents = schedules.Select(schedule => schedule.Key);
			var maxSeatData = _maxSeatSkillDataFactory.Create(period, agentsToOptimize, scenario, allAgents, maxSeatIntervalLength);
			if (!maxSeatData.MaxSeatSkillExists())
				return;
			var tagSetter = optimizationPreferences.General.ScheduleTag == null ?
				new ScheduleTagSetter(NullScheduleTag.Instance) :
				new ScheduleTagSetter(optimizationPreferences.General.ScheduleTag);
			var allMatrixes = _matrixListFactory.CreateMatrixListAllForLoadedPeriod(schedules, allAgents, period);
			var businessRules = NewBusinessRuleCollection.Minimum();
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
			_teamInfoFactoryFactory.Create(allAgents, schedules, optimizationPreferences.Extra.TeamGroupPage);
			var teamBlockInfos = _teamBlockGenerator.Generate(allAgents, allMatrixes, period, agentsToOptimize, schedulingOptions);

			using (_resourceCalculationContextFactory.Create(schedules, maxSeatData.AllMaxSeatSkills()))
			{
				foreach (var teamBlockInfo in teamBlockInfos)
				{
					var datePoint = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection().FirstOrDefault(x => x >= period.StartDate); //what is this?
					var skillDaysForTeamBlockInfo = maxSeatData.SkillDaysFor(teamBlockInfo, datePoint); //This won't work if not hiearchy
					var maxPeakBefore = _maxSeatPeak.Fetch(datePoint, maxSeatData, skillDaysForTeamBlockInfo);
					if (maxPeakBefore > 0.01)
					{
						var rollbackService = new SchedulePartModifyAndRollbackService(null, _scheduleDayChangeCallback, tagSetter);
						_teamBlockClearer.ClearTeamBlockWithNoResourceCalculation(rollbackService, teamBlockInfo, businessRules); //TODO: fix - don't remove everyone if not needed
						if (!_teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelectorForMaxSeat,
													teamBlockInfo,
													datePoint,
													schedulingOptions,
													rollbackService,
													new DoNothingResourceCalculateDelayer(),
													skillDaysForTeamBlockInfo,
													schedules,
													new ShiftNudgeDirective(),
													businessRules) ||
								!_restrictionOverLimitValidator.Validate(teamBlockInfo.MatrixesForGroupAndBlock(), optimizationPreferences) ||
								!_teamBlockShiftCategoryLimitationValidator.Validate(teamBlockInfo, null, optimizationPreferences) ||
								_maxSeatPeak.Fetch(datePoint, maxSeatData, skillDaysForTeamBlockInfo) > maxPeakBefore)
						{
							rollbackService.RollbackMinimumChecks();
						}
					}
				}
			}
		}
	}
}