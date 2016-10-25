using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
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
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly IMatrixUserLockLocker _matrixUserLockLocker;
		private readonly IMatrixNotPermittedLocker _matrixNotPermittedLocker;
		private readonly ITeamBlockGenerator _teamBlockGenerator;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly WorkShiftSelectorForMaxSeat _workShiftSelectorForMaxSeat;
		private readonly IGroupPersonBuilderForOptimizationFactory _groupPersonBuilderForOptimizationFactory;

		public MaxSeatOptimization(MaxSeatSkillDataFactory maxSeatSkillDataFactory,
														CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
														IScheduleDayChangeCallback scheduleDayChangeCallback,
														ISchedulingOptionsCreator schedulingOptionsCreator,
														Func<ISchedulerStateHolder> stateHolder, //should be removed!
														ITeamBlockScheduler teamBlockScheduler,
														IMatrixUserLockLocker matrixUserLockLocker,
														IMatrixNotPermittedLocker matrixNotPermittedLocker,
														ITeamBlockGenerator teamBlockGenerator,
														ITeamBlockClearer teamBlockClearer,
														WorkShiftSelectorForMaxSeat workShiftSelectorForMaxSeat,
														IGroupPersonBuilderForOptimizationFactory groupPersonBuilderForOptimizationFactory)
		{
			_maxSeatSkillDataFactory = maxSeatSkillDataFactory;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_stateHolder = stateHolder;
			_teamBlockScheduler = teamBlockScheduler;
			_matrixUserLockLocker = matrixUserLockLocker;
			_matrixNotPermittedLocker = matrixNotPermittedLocker;
			_teamBlockGenerator = teamBlockGenerator;
			_teamBlockClearer = teamBlockClearer;
			_workShiftSelectorForMaxSeat = workShiftSelectorForMaxSeat;
			_groupPersonBuilderForOptimizationFactory = groupPersonBuilderForOptimizationFactory;
		}

		public void Optimize(DateOnlyPeriod period, IEnumerable<IPerson> agentsToOptimize, IScheduleDictionary schedules, IScenario scenario, IOptimizationPreferences optimizationPreferences)
		{
			var allAgents = schedules.Select(schedule => schedule.Key);
			var maxSeatData = _maxSeatSkillDataFactory.Create(period, agentsToOptimize, scenario, allAgents);

			//TODO: REMOVE! //
			var loadedPeriod = schedules.Period.LoadedPeriod(); //FIX!
			((SchedulerStateHolder)_stateHolder()).SetLoadedPeriod_UseOnlyFromTest_ShouldProbablyBePutOnScheduleDictionaryInsteadIfNeededAtAll(loadedPeriod); //needed to build groups
			allAgents.ForEach(x => _stateHolder().AllPermittedPersons.Add(x)); //needed to build groups
			_stateHolder().SchedulingResultState.Schedules = schedules;
			//////////////////

			var tagSetter = new ScheduleTagSetter(new NullScheduleTag()); //fix - the tag
			var rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder().SchedulingResultState, //fix!
				_scheduleDayChangeCallback,
				tagSetter);

			var allMatrixes = createMatrixes(schedules, 
				loadedPeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc) //FIX
				, period, allAgents);

			using (_resourceCalculationContextFactory.Create(schedules, maxSeatData.AllMaxSeatSkills()))
			{
				teamBlockIntradayOptimizationService(
					allMatrixes,
					period,
					agentsToOptimize,
					optimizationPreferences,
					rollbackService,
					maxSeatData.AllMaxSeatSkillDaysPerSkill(), 
					schedules,
					allAgents,
					NewBusinessRuleCollection.Minimum()); //is this enough?
			}
		}

		private IEnumerable<IScheduleMatrixPro> createMatrixes(IScheduleDictionary scheduleDictionary, DateOnlyPeriod loadedPeriod, DateOnlyPeriod choosenPeriod, IEnumerable<IPerson> allAgents)
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
		private void teamBlockIntradayOptimizationService(IEnumerable<IScheduleMatrixPro> allPersonMatrixList,
			DateOnlyPeriod selectedPeriod,
			IEnumerable<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays,
			IScheduleDictionary schedules,
			IEnumerable<IPerson> personsInOrganization,
			INewBusinessRuleCollection businessRuleCollection)
		{
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
			_groupPersonBuilderForOptimizationFactory.Create(optimizationPreferences.Extra.TeamGroupPage);
			var teamBlocks = _teamBlockGenerator.Generate(personsInOrganization, allPersonMatrixList, selectedPeriod, selectedPersons, schedulingOptions);
			var remainingInfoList = teamBlocks.ToList();

			while (remainingInfoList.Count > 0)
			{
				optimizeOneRound(selectedPeriod,
					schedulingOptions, remainingInfoList,
					schedulePartModifyAndRollbackService,
					skillDays, schedules, businessRuleCollection);
			}
		}

		private void optimizeOneRound(DateOnlyPeriod selectedPeriod, ISchedulingOptions schedulingOptions,
			ICollection<ITeamBlockInfo> allTeamBlockInfos, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, 
			IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, IScheduleDictionary schedules, INewBusinessRuleCollection businessRuleCollection)
		{
			foreach (var teamBlockInfo in allTeamBlockInfos.ToList())
			{
				var firstSelectedDay = selectedPeriod.DayCollection().First();
				var datePoint = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection().FirstOrDefault(x => x >= firstSelectedDay);
				_teamBlockClearer.ClearTeamBlockWithNoResourceCalculation(schedulePartModifyAndRollbackService, teamBlockInfo);
				_teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelectorForMaxSeat, teamBlockInfo, datePoint, schedulingOptions,
					schedulePartModifyAndRollbackService,
					new DoNothingResourceCalculateDelayer(), skillDays.ToSkillDayEnumerable(), schedules, new ShiftNudgeDirective(), businessRuleCollection);

				allTeamBlockInfos.Remove(teamBlockInfo);
			}
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