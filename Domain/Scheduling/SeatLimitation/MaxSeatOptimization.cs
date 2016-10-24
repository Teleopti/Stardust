using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
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
		private readonly Func<ISchedulerStateHolder> _stateHolder;
		private readonly IResourceOptimization _resourceOptimization;
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly ITeamBlockIntradayDecisionMaker _teamBlockIntradayDecisionMaker;
		private readonly ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;
		private readonly TeamInfoFactoryFactory _teamInfoFactoryFactory;
		private readonly IMatrixUserLockLocker _matrixUserLockLocker;
		private readonly IMatrixNotPermittedLocker _matrixNotPermittedLocker;
		private readonly ITeamBlockGenerator _teamBlockGenerator;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly WorkShiftSelectorForMaxSeat _workShiftSelectorForMaxSeat;

		public MaxSeatOptimization(MaxSeatSkillDataFactory maxSeatSkillDataFactory,
														CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
														IScheduleDayChangeCallback scheduleDayChangeCallback,
														ISchedulingOptionsCreator schedulingOptionsCreator,
														Func<ISchedulerStateHolder> stateHolder, //should be removed!
														IResourceOptimization resourceOptimization,
														ITeamBlockScheduler teamBlockScheduler,
														ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
														ITeamBlockIntradayDecisionMaker teamBlockIntradayDecisionMaker,
														ITeamBlockShiftCategoryLimitationValidator teamBlockShiftCategoryLimitationValidator,
														TeamInfoFactoryFactory teamInfoFactoryFactory,
														IMatrixUserLockLocker matrixUserLockLocker,
														IMatrixNotPermittedLocker matrixNotPermittedLocker,
														ITeamBlockGenerator teamBlockGenerator,
														ITeamBlockClearer teamBlockClearer,
														WorkShiftSelectorForMaxSeat workShiftSelectorForMaxSeat)
		{
			_maxSeatSkillDataFactory = maxSeatSkillDataFactory;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_stateHolder = stateHolder;
			_resourceOptimization = resourceOptimization;
			_teamBlockScheduler = teamBlockScheduler;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_teamBlockIntradayDecisionMaker = teamBlockIntradayDecisionMaker;
			_teamBlockShiftCategoryLimitationValidator = teamBlockShiftCategoryLimitationValidator;
			_teamInfoFactoryFactory = teamInfoFactoryFactory;
			_matrixUserLockLocker = matrixUserLockLocker;
			_matrixNotPermittedLocker = matrixNotPermittedLocker;
			_teamBlockGenerator = teamBlockGenerator;
			_teamBlockClearer = teamBlockClearer;
			_workShiftSelectorForMaxSeat = workShiftSelectorForMaxSeat;
		}

		public void Optimize(DateOnlyPeriod period, IEnumerable<IPerson> agentsToOptimize, IScheduleDictionary schedules, IScenario scenario, IOptimizationPreferences optimizationPreferences)
		{
			var allAgents = schedules.Select(schedule => schedule.Key);
			var maxSeatData = _maxSeatSkillDataFactory.Create(period, agentsToOptimize, scenario, allAgents);
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);

			//TODO: REMOVE! //
			var loadedPeriod = schedules.Period.LoadedPeriod(); //FIX!
			((SchedulerStateHolder)_stateHolder()).SetLoadedPeriod_UseOnlyFromTest_ShouldProbablyBePutOnScheduleDictionaryInsteadIfNeededAtAll(loadedPeriod); //needed to build groups
			allAgents.ForEach(x => _stateHolder().SchedulingResultState.PersonsInOrganization.Add(x)); //needed to build groups
			allAgents.ForEach(x => _stateHolder().AllPermittedPersons.Add(x)); //needed to build groups
			_stateHolder().SchedulingResultState.SkillDays = maxSeatData.AllMaxSeatSkillDaysPerSkill(); //needed forTeamBlockMaxSeatChecker
			_stateHolder().SchedulingResultState.Schedules = schedules;
			//////////////////

			optimizationPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak;

			var tagSetter = new ScheduleTagSetter(new NullScheduleTag());
			var rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder().SchedulingResultState, //fix!
				_scheduleDayChangeCallback,
				tagSetter);
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimization, 1, schedulingOptions.ConsiderShortBreaks, _stateHolder().SchedulingResultState); //fix!
			var teamInfoFactory = _teamInfoFactoryFactory.Create(schedulingOptions.GroupOnGroupPageForTeamBlockPer); //FIX - why is this needed!?

			var allMatrixes = createMatrixes(schedules, 
				loadedPeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc) //FIX
				, period, allAgents);

			using (_resourceCalculationContextFactory.Create(schedules, maxSeatData.AllMaxSeatSkills()))
			{
				teamBlockIntradayOptimizationService(
					allMatrixes,
					period,
					agentsToOptimize.ToList(),
					optimizationPreferences,
					rollbackService,
					resourceCalculateDelayer,
					maxSeatData.AllMaxSeatSkillDaysPerSkill(), //fix
					NewBusinessRuleCollection.Minimum()); //is this enough?
			}


		}

		private IList<IScheduleMatrixPro> createMatrixes(IScheduleDictionary scheduleDictionary, DateOnlyPeriod loadedPeriod, DateOnlyPeriod choosenPeriod, IEnumerable<IPerson> allAgents)
		{
			//old: _matrixListFactory.CreateMatrixListAllForLoadedPeriod(period); - it's currently depending on stateholder though
			//move to seperate class!

			var period = loadedPeriod.Inflate(10);
			var persons = allAgents;
			var startDate = period.StartDate;
			var matrixes = new List<IScheduleMatrixPro>();
			foreach (var person in persons)
			{
				var date = startDate;
				while (date <= period.EndDate)
				{
					var matrix = createMatrixForPersonAndDate(scheduleDictionary, person, date);
					if (matrix == null)
					{
						date = date.AddDays(1);
						continue;
					}
					matrixes.Add(matrix);
					date = matrix.SchedulePeriod.DateOnlyPeriod.EndDate.AddDays(1);
				}
			}
			_matrixUserLockLocker.Execute(matrixes, choosenPeriod);
			_matrixNotPermittedLocker.Execute(matrixes);


			return matrixes;
		}


		#region  taken from TeamBlockIntradayOptimizationService
		public void teamBlockIntradayOptimizationService(IList<IScheduleMatrixPro> allPersonMatrixList,
			DateOnlyPeriod selectedPeriod,
			IList<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays,
			INewBusinessRuleCollection businessRuleCollection)
		{
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
			var teamBlocks = _teamBlockGenerator.Generate(allPersonMatrixList, selectedPeriod, selectedPersons, schedulingOptions);
			var remainingInfoList = new List<ITeamBlockInfo>(teamBlocks);

			while (remainingInfoList.Count > 0)
			{
				var teamBlocksToRemove = optimizeOneRound(selectedPeriod, optimizationPreferences,
					schedulingOptions, remainingInfoList,
					schedulePartModifyAndRollbackService,
					resourceCalculateDelayer,
					skillDays, businessRuleCollection);
				foreach (var teamBlock in teamBlocksToRemove)
				{
					remainingInfoList.Remove(teamBlock);
				}
			}
		}

		private IEnumerable<ITeamBlockInfo> optimizeOneRound(DateOnlyPeriod selectedPeriod,
			IOptimizationPreferences optimizationPreferences, ISchedulingOptions schedulingOptions,
			IList<ITeamBlockInfo> allTeamBlockInfos, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, INewBusinessRuleCollection businessRuleCollection)
		{
			var teamBlockToRemove = new List<ITeamBlockInfo>();

			//var sortedTeamBlockInfos = _teamBlockIntradayDecisionMaker.Decide(allTeamBlockInfos, optimizationPreferences, schedulingOptions);

			foreach (var teamBlockInfo in allTeamBlockInfos)
			{
				//if (!_teamTeamBlockSteadyStateValidator.IsTeamBlockInSteadyState(teamBlockInfo, schedulingOptions))
				//{
				//	teamBlockToRemove.Add(teamBlockInfo);
				//	continue;
				//}

				//schedulePartModifyAndRollbackService.ClearModificationCollection();

				var firstSelectedDay = selectedPeriod.DayCollection().First();
				var datePoint = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection().FirstOrDefault(x => x >= firstSelectedDay);
				//if (_teamBlockMaxSeatChecker.CheckMaxSeat(datePoint, schedulingOptions, teamBlockInfo.TeamInfo, skillDays)) change row ~40, remove || skillPair.Key != maxSeatSkill
				//{
				//	teamBlockToRemove.Add(teamBlockInfo);
				//	continue;
				//}

				//var previousTargetValue = _dailyTargetValueCalculatorForTeamBlock.TargetValue(teamBlockInfo, optimizationPreferences.Advanced);
				_teamBlockClearer.ClearTeamBlock(schedulingOptions, schedulePartModifyAndRollbackService, teamBlockInfo);




				var success = _teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelectorForMaxSeat, teamBlockInfo, datePoint, schedulingOptions,
					schedulePartModifyAndRollbackService,
					resourceCalculateDelayer, skillDays.ToSkillDayEnumerable(), new ShiftNudgeDirective(), businessRuleCollection);
				//if (!success)
				//{
				//	teamBlockToRemove.Add(teamBlockInfo);
				//	_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
				//	continue;
				//}

				//if (!_teamBlockMaxSeatChecker.CheckMaxSeat(datePoint, schedulingOptions, teamBlockInfo.TeamInfo) || !_teamBlockOptimizationLimits.Validate(teamBlockInfo.MatrixesForGroupAndBlock(), optimizationPreferences))
				//{
				//	var progressResult = onReportProgress(new ResourceOptimizerProgressEventArgs(0, 0, Resources.OptimizingIntraday + Resources.Colon + Resources.RollingBackSchedulesFor + " " + teamBlockInfo.BlockInfo.BlockPeriod.DateString + " " + teamName,cancelAction));
				//	teamBlockToRemove.Add(teamBlockInfo);
				//	_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
				//	if (progressResult.ShouldCancel)
				//	{
				//		cancelAction();
				//		break;
				//	}
				//	continue;
				//}

				//if (!_teamBlockShiftCategoryLimitationValidator.Validate(teamBlockInfo, null, optimizationPreferences))
				//{
				//	teamBlockToRemove.Add(teamBlockInfo);
				//	_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
				//	continue;
				//}


				//var newTargetValue = _dailyTargetValueCalculatorForTeamBlock.TargetValue(teamBlockInfo, optimizationPreferences.Advanced);
				teamBlockToRemove.Add(teamBlockInfo);
			}
			return teamBlockToRemove;
		}
#endregion


		private static IScheduleMatrixPro createMatrixForPersonAndDate(IScheduleDictionary scheduleDictionary, IPerson person, DateOnly date)
		{
			//old: _matrixListFactory.CreateMatrixListAllForLoadedPeriod(period); - it's currently depending on stateholder though
			var virtualSchedulePeriod = person.VirtualSchedulePeriod(date);
			if (!virtualSchedulePeriod.IsValid)
				return null;

			IFullWeekOuterWeekPeriodCreator fullWeekOuterWeekPeriodCreator =
				new FullWeekOuterWeekPeriodCreator(virtualSchedulePeriod.DateOnlyPeriod, person);

			return new ScheduleMatrixPro(scheduleDictionary[person],
				fullWeekOuterWeekPeriodCreator,
				virtualSchedulePeriod);
		}
	}
}