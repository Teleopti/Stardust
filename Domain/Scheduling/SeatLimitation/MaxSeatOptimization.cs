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
														ITeamBlockScheduler teamBlockScheduler,
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
			_teamBlockScheduler = teamBlockScheduler;
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
			_stateHolder().SchedulingResultState.SkillDays = maxSeatData.AllMaxSeatSkillDaysPerSkill(); //needed for SameOpenHoursInTeamBlockSpecification
			_stateHolder().SchedulingResultState.Schedules = schedules;
			//////////////////

		//	optimizationPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak;

			var tagSetter = new ScheduleTagSetter(new NullScheduleTag());
			var rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder().SchedulingResultState, //fix!
				_scheduleDayChangeCallback,
				tagSetter);
			var teamInfoFactory = _teamInfoFactoryFactory.Create(schedulingOptions.GroupOnGroupPageForTeamBlockPer); //FIX - why is this needed!?

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
					maxSeatData.AllMaxSeatSkillDaysPerSkill(), //fix
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
			INewBusinessRuleCollection businessRuleCollection)
		{
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
			var teamBlocks = _teamBlockGenerator.Generate(allPersonMatrixList, selectedPeriod, selectedPersons, schedulingOptions);
			var remainingInfoList = teamBlocks.ToList();

			while (remainingInfoList.Count > 0)
			{
				optimizeOneRound(selectedPeriod,
					schedulingOptions, remainingInfoList,
					schedulePartModifyAndRollbackService,
					skillDays, businessRuleCollection);
			}
		}

		private void optimizeOneRound(DateOnlyPeriod selectedPeriod, ISchedulingOptions schedulingOptions,
			ICollection<ITeamBlockInfo> allTeamBlockInfos, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, INewBusinessRuleCollection businessRuleCollection)
		{
			foreach (var teamBlockInfo in allTeamBlockInfos.ToList())
			{
				var firstSelectedDay = selectedPeriod.DayCollection().First();
				var datePoint = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection().FirstOrDefault(x => x >= firstSelectedDay);
				_teamBlockClearer.ClearTeamBlockWithNoResourceCalculation(schedulePartModifyAndRollbackService, teamBlockInfo);
				_teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelectorForMaxSeat, teamBlockInfo, datePoint, schedulingOptions,
					schedulePartModifyAndRollbackService,
					new DoNothingResourceCalculateDelayer(), skillDays.ToSkillDayEnumerable(), new ShiftNudgeDirective(), businessRuleCollection);

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