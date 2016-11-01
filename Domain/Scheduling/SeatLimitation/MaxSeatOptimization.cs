using System;
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

		public void Optimize(DateOnlyPeriod period, IEnumerable<IPerson> agentsToOptimize, IScheduleDictionary schedules, IScenario scenario, IOptimizationPreferences optimizationPreferences)
		{
			var allAgents = schedules.Select(schedule => schedule.Key); //blir det rätt att inte ta med schemalagda agenter?
			var maxSeatData = _maxSeatSkillDataFactory.Create(period, agentsToOptimize, scenario, allAgents);
			if (!maxSeatData.MaxSeatSkillExists())
				return;
			var tagSetter = new ScheduleTagSetter(new NullScheduleTag()); //fix - the tag
			var rollbackService = new SchedulePartModifyAndRollbackService(null, _scheduleDayChangeCallback, tagSetter);
			var allMatrixes = _matrixListFactory.CreateMatrixListAllForLoadedPeriod(schedules, allAgents, period);
			var businessRules = NewBusinessRuleCollection.Minimum(); //is this enough?

			using (_resourceCalculationContextFactory.Create(schedules, maxSeatData.AllMaxSeatSkills()))
			{
				//most stuff taken from TeamBlockIntradayOptimizationService
				var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
				var teamInfoFactory_shouldWeUseThisOne = _teamInfoFactoryFactory.Create(allAgents, schedules, optimizationPreferences.Extra.TeamGroupPage);
				var teamBlocks = _teamBlockGenerator.Generate(allAgents, allMatrixes, period, agentsToOptimize, schedulingOptions);
				var remainingInfoList = teamBlocks.ToList();

				while (remainingInfoList.Count > 0)
				{
					foreach (var teamBlockInfo in remainingInfoList.ToList())
					{
						var firstSelectedDay = period.DayCollection().First();
						var datePoint = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection().FirstOrDefault(x => x >= firstSelectedDay);
						var maxPeakBefore = _maxSeatPeak.Fetch(datePoint, teamBlockInfo, maxSeatData.AllMaxSeatSkillDaysPerSkill());

						if (Math.Abs(maxPeakBefore) < 0.0001)
						{
							remainingInfoList.Remove(teamBlockInfo);
							continue;
						}

						_teamBlockClearer.ClearTeamBlockWithNoResourceCalculation(rollbackService, teamBlockInfo, businessRules); //TODO: fix - dont do this

						if (!_teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelectorForMaxSeat, teamBlockInfo, datePoint,
								schedulingOptions,
								rollbackService,
								new DoNothingResourceCalculateDelayer(), maxSeatData.AllMaxSeatSkillDaysPerSkill().ToSkillDayEnumerable(),
								schedules, new ShiftNudgeDirective(), businessRules) ||
								!_restrictionOverLimitValidator.Validate(teamBlockInfo.MatrixesForGroupAndBlock(), optimizationPreferences) ||
								!_teamBlockShiftCategoryLimitationValidator.Validate(teamBlockInfo, null, optimizationPreferences) ||
								_maxSeatPeak.Fetch(datePoint, teamBlockInfo, maxSeatData.AllMaxSeatSkillDaysPerSkill()) > maxPeakBefore)
						{
							rollbackService.RollbackMinimumChecks(); //förmodligen fel - rullar tillbaka allt
						}

						remainingInfoList.Remove(teamBlockInfo);
					}
				}
			}
		}
	}
}