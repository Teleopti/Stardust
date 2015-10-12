using System;
using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Web.Areas.Global;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class OptimizationController : ApiController
    {
		private readonly SetupStateHolderForWebScheduling _setupStateHolderForWebScheduling;
		private readonly IFixedStaffLoader _fixedStaffLoader;
		private readonly IActionThrottler _actionThrottler;
		private readonly IScheduleControllerPrerequisites _prerequisites;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IClassicDaysOffOptimizationCommand _classicDaysOffOptimizationCommand;
		private readonly Func<IPersonSkillProvider> _personSkillProvider;
		private readonly IScheduleDictionaryPersister _persister;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;

		public OptimizationController(SetupStateHolderForWebScheduling setupStateHolderForWebScheduling,
			IFixedStaffLoader fixedStaffLoader, IActionThrottler actionThrottler, IScheduleControllerPrerequisites prerequisites, Func<ISchedulerStateHolder> schedulerStateHolder,
			IClassicDaysOffOptimizationCommand classicDaysOffOptimizationCommand,
			Func<IPersonSkillProvider> personSkillProvider, IScheduleDictionaryPersister persister, IPlanningPeriodRepository planningPeriodRepository)
		{
			_setupStateHolderForWebScheduling = setupStateHolderForWebScheduling;
			_fixedStaffLoader = fixedStaffLoader;
			_actionThrottler = actionThrottler;
			_prerequisites = prerequisites;
			_schedulerStateHolder = schedulerStateHolder;
			_classicDaysOffOptimizationCommand = classicDaysOffOptimizationCommand;
			_personSkillProvider = personSkillProvider;
			_persister = persister;
			_planningPeriodRepository = planningPeriodRepository;
		}

		[HttpPost, Route("api/ResourcePlanner/optimize/FixedStaff/{id}"), Authorize, UnitOfWork]
		public virtual IHttpActionResult FixedStaff(Guid id, BlockToken tokenFromScheduling)
		{
			_actionThrottler.Resume(tokenFromScheduling);
			try
			{
				var planningPeriod = _planningPeriodRepository.Load(id);

				var period = new DateOnlyPeriod(new DateOnly(planningPeriod.Range.StartDate.Date),
					new DateOnly(planningPeriod.Range.EndDate.Date));

				_prerequisites.MakeSureLoaded();

				var people = _fixedStaffLoader.Load(period);

				_setupStateHolderForWebScheduling.Setup(period, people);

				var allSchedules = extractAllSchedules(_schedulerStateHolder().SchedulingResultState, people, period);
				initializePersonSkillProviderBeforeAccessingItFromOtherThreads(period, people.AllPeople);
				var optimizationPreferences = new OptimizationPreferences
				{
					DaysOff =
						new DaysOffPreferences
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
					General = new GeneralPreferences {ScheduleTag = NullScheduleTag.Instance, OptimizationStepDaysOff = true}
				};
				_classicDaysOffOptimizationCommand.Execute(allSchedules, period, optimizationPreferences, _schedulerStateHolder(),
					new NoBackgroundWorker());

				_persister.Persist(_schedulerStateHolder().Schedules);

				planningPeriod.Scheduled();

				var result = new OptimizationResultModel();
				result.Map(_schedulerStateHolder().SchedulingResultState.SkillDays, period);
				return Ok(result);
			}
			finally
			{
				_actionThrottler.Finish(tokenFromScheduling);
			}
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

		private void initializePersonSkillProviderBeforeAccessingItFromOtherThreads(DateOnlyPeriod period,
			IEnumerable<IPerson> allPeople)
		{
			var provider = _personSkillProvider();
			var dayCollection = period.DayCollection();
			allPeople.ForEach(p => dayCollection.ForEach(d => provider.SkillsOnPersonDate(p, d)));
		}
    }
}