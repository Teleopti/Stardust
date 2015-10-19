using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class ScheduleOptimization
	{
		private readonly SetupStateHolderForWebScheduling _setupStateHolderForWebScheduling;
		private readonly IFixedStaffLoader _fixedStaffLoader;
		private readonly IScheduleControllerPrerequisites _prerequisites;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IClassicDaysOffOptimizationCommand _classicDaysOffOptimizationCommand;
		private readonly Func<IPersonSkillProvider> _personSkillProvider;
		private readonly IScheduleDictionaryPersister _persister;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly WeeklyRestSolverExecuter _weeklyRestSolverExecuter;

		public ScheduleOptimization(SetupStateHolderForWebScheduling setupStateHolderForWebScheduling,
	IFixedStaffLoader fixedStaffLoader, IScheduleControllerPrerequisites prerequisites, Func<ISchedulerStateHolder> schedulerStateHolder,
	IClassicDaysOffOptimizationCommand classicDaysOffOptimizationCommand,
	Func<IPersonSkillProvider> personSkillProvider, IScheduleDictionaryPersister persister, IPlanningPeriodRepository planningPeriodRepository,
	WeeklyRestSolverExecuter weeklyRestSolverExecuter)
		{
			_setupStateHolderForWebScheduling = setupStateHolderForWebScheduling;
			_fixedStaffLoader = fixedStaffLoader;
			_prerequisites = prerequisites;
			_schedulerStateHolder = schedulerStateHolder;
			_classicDaysOffOptimizationCommand = classicDaysOffOptimizationCommand;
			_personSkillProvider = personSkillProvider;
			_persister = persister;
			_planningPeriodRepository = planningPeriodRepository;
			_weeklyRestSolverExecuter = weeklyRestSolverExecuter;
		}

		public OptimizationResultModel Execute(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);

			var period = planningPeriod.Range;

			_prerequisites.MakeSureLoaded();

			var people = _fixedStaffLoader.Load(period);

			_setupStateHolderForWebScheduling.Setup(period, people);

			var allSchedules = extractAllSchedules(_schedulerStateHolder().SchedulingResultState, people, period);
			initializePersonSkillProviderBeforeAccessingItFromOtherThreads(period, people.AllPeople);
			var optimizationPreferences = new OptimizationPreferences
			{
				DaysOff = new DaysOffPreferences
					{
						ConsecutiveDaysOffValue = new MinMax<int>(1, 3),
						UseConsecutiveDaysOff = true,
						ConsecutiveWorkdaysValue = new MinMax<int>(2, 6),
						UseConsecutiveWorkdays = true,
						ConsiderWeekAfter = true,
						ConsiderWeekBefore = true,
						DaysOffPerWeekValue = new MinMax<int>(1, 3),
						UseDaysOffPerWeek = true
					},
				General = new GeneralPreferences { ScheduleTag = NullScheduleTag.Instance, OptimizationStepDaysOff = true }
			};
			_classicDaysOffOptimizationCommand.Execute(allSchedules, period, optimizationPreferences, _schedulerStateHolder(), new NoBackgroundWorker());

			_weeklyRestSolverExecuter.Resolve(optimizationPreferences, period, allSchedules, people.AllPeople);

			_persister.Persist(_schedulerStateHolder().Schedules);

			planningPeriod.Scheduled();

			var result = new OptimizationResultModel();
			result.Map(_schedulerStateHolder().SchedulingResultState.SkillDays, period);
			return result;
		}


		private static IList<IScheduleDay> extractAllSchedules(ISchedulingResultStateHolder stateHolder,
			PeopleSelection people,
			DateOnlyPeriod period)
		{
			var allSchedules = new List<IScheduleDay>();
			foreach (var schedule in stateHolder.Schedules)
			{
				if (people.FixedStaffPeople.Contains(schedule.Key))
				{
					allSchedules.AddRange(schedule.Value.ScheduledDayCollection(period));
				}
			}
			return allSchedules;
		}

		private void initializePersonSkillProviderBeforeAccessingItFromOtherThreads(DateOnlyPeriod period, IEnumerable<IPerson> allPeople)
		{
			var provider = _personSkillProvider();
			var dayCollection = period.DayCollection();
			allPeople.ForEach(p => dayCollection.ForEach(d => provider.SkillsOnPersonDate(p, d)));
		}
	}
}