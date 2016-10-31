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
		private readonly IGroupPersonBuilderForOptimizationFactory _groupPersonBuilderForOptimizationFactory;
		private readonly ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;
		private readonly RestrictionOverLimitValidator _restrictionOverLimitValidator;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly ITeamBlockMaxSeatChecker _teamBlockMaxSeatChecker;

		public MaxSeatOptimization(MaxSeatSkillDataFactory maxSeatSkillDataFactory,
														CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
														IScheduleDayChangeCallback scheduleDayChangeCallback,
														ISchedulingOptionsCreator schedulingOptionsCreator,
														ITeamBlockScheduler teamBlockScheduler,
														ITeamBlockGenerator teamBlockGenerator,
														ITeamBlockClearer teamBlockClearer,
														WorkShiftSelectorForMaxSeat workShiftSelectorForMaxSeat,
														IGroupPersonBuilderForOptimizationFactory groupPersonBuilderForOptimizationFactory,
														ITeamBlockShiftCategoryLimitationValidator teamBlockShiftCategoryLimitationValidator,
														RestrictionOverLimitValidator restrictionOverLimitValidator,
														IMatrixListFactory matrixListFactory,
														ITeamBlockMaxSeatChecker teamBlockMaxSeatChecker)
		{
			_maxSeatSkillDataFactory = maxSeatSkillDataFactory;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_teamBlockScheduler = teamBlockScheduler;
			_teamBlockGenerator = teamBlockGenerator;
			_teamBlockClearer = teamBlockClearer;
			_workShiftSelectorForMaxSeat = workShiftSelectorForMaxSeat;
			_groupPersonBuilderForOptimizationFactory = groupPersonBuilderForOptimizationFactory;
			_teamBlockShiftCategoryLimitationValidator = teamBlockShiftCategoryLimitationValidator;
			_restrictionOverLimitValidator = restrictionOverLimitValidator;
			_matrixListFactory = matrixListFactory;
			_teamBlockMaxSeatChecker = teamBlockMaxSeatChecker;
		}

		public void Optimize(DateOnlyPeriod period, IEnumerable<IPerson> agentsToOptimize, IScheduleDictionary schedules, IScenario scenario, IOptimizationPreferences optimizationPreferences)
		{
			var allAgents = schedules.Select(schedule => schedule.Key); //blir det rätt att inte ta med schemalagda agenter?
			var maxSeatData = _maxSeatSkillDataFactory.Create(period, agentsToOptimize, scenario, allAgents);
			var tagSetter = new ScheduleTagSetter(new NullScheduleTag()); //fix - the tag
			var rollbackService = new SchedulePartModifyAndRollbackService(null, _scheduleDayChangeCallback, tagSetter);
			var allMatrixes = _matrixListFactory.CreateMatrixListAllForLoadedPeriod(schedules, allAgents, period);
			var businessRules = NewBusinessRuleCollection.Minimum(); //is this enough?

			using (_resourceCalculationContextFactory.Create(schedules, maxSeatData.AllMaxSeatSkills()))
			{
				
				//most stuff taken from TeamBlockIntradayOptimizationService
				var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
				_groupPersonBuilderForOptimizationFactory.Create(allAgents, schedules, optimizationPreferences.Extra.TeamGroupPage);
				var teamBlocks = _teamBlockGenerator.Generate(allAgents, allMatrixes, period, agentsToOptimize, schedulingOptions);
				var remainingInfoList = teamBlocks.ToList();

				while (remainingInfoList.Count > 0)
				{
					foreach (var teamBlockInfo in remainingInfoList.ToList())
					{
						var firstSelectedDay = period.DayCollection().First();
						var datePoint = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection().FirstOrDefault(x => x >= firstSelectedDay);

						if (_teamBlockMaxSeatChecker.CheckMaxSeat(datePoint,
																new SchedulingOptions { UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak },
																teamBlockInfo.TeamInfo,
																maxSeatData.AllMaxSeatSkillDaysPerSkill()))
						{
							remainingInfoList.Remove(teamBlockInfo);
							continue;
						}

						_teamBlockClearer.ClearTeamBlockWithNoResourceCalculation(rollbackService, teamBlockInfo, businessRules); //TODO: check if this is enough

						_teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelectorForMaxSeat, teamBlockInfo, datePoint, schedulingOptions,
							rollbackService,
							new DoNothingResourceCalculateDelayer(), maxSeatData.AllMaxSeatSkillDaysPerSkill().ToSkillDayEnumerable(), schedules, new ShiftNudgeDirective(), businessRules);

						if (!_restrictionOverLimitValidator.Validate(teamBlockInfo.MatrixesForGroupAndBlock(), optimizationPreferences))
						{
							//kolla om vi ska ändra "gamla" rollback istället
							rollbackService.RollbackMinimumChecks(); //förmodligen fel - rullar tillbaka allt	
						}

						if (!_teamBlockShiftCategoryLimitationValidator.Validate(teamBlockInfo, null, optimizationPreferences)) //kolla null
						{
							//kolla om vi ska ändra "gamla" rollback istället
							rollbackService.RollbackMinimumChecks(); //förmodligen fel - rullar tillbaka allt
						} 

						remainingInfoList.Remove(teamBlockInfo);
					}
				}
			}
		}
	}
}