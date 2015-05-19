using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.WebTest.Areas.ResourcePlanner;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class ScheduleController : ApiController
	{
		private readonly SetupStateHolderForWebScheduling _setupStateHolderForWebScheduling;
		private readonly FixedStaffLoader _fixedStaffLoader;
		private readonly IDayOffTemplateRepository _dayOffTemplateRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly Func<IFixedStaffSchedulingService> _fixedStaffSchedulingService;
		private readonly Func<IScheduleCommand> _scheduleCommand;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly Func<IRequiredScheduleHelper> _requiredScheduleHelper;
		private readonly Func<IGroupPagePerDateHolder> _groupPagePerDateHolder;
		private readonly Func<IScheduleTagSetter> _scheduleTagSetter;
		private readonly IScheduleRangePersister _persister;
		private readonly Func<IPersonSkillProvider> _personSkillProvider;
		private readonly ViolatedSchedulePeriodBusinessRule _violatedSchedulePeriodBusinessRule;

		public ScheduleController(SetupStateHolderForWebScheduling setupStateHolderForWebScheduling,FixedStaffLoader fixedStaffLoader, IDayOffTemplateRepository dayOffTemplateRepository,
					IActivityRepository activityRepository, Func<IFixedStaffSchedulingService> fixedStaffSchedulingService,Func<IScheduleCommand> scheduleCommand, Func<ISchedulerStateHolder> schedulerStateHolder,
						Func<IRequiredScheduleHelper> requiredScheduleHelper, Func<IGroupPagePerDateHolder> groupPagePerDateHolder,Func<IScheduleTagSetter> scheduleTagSetter, 
							Func<IPersonSkillProvider> personSkillProvider,IScheduleRangePersister persister,
									ViolatedSchedulePeriodBusinessRule violatedSchedulePeriodBusinessRule)
		{
			_setupStateHolderForWebScheduling = setupStateHolderForWebScheduling;
			_fixedStaffLoader = fixedStaffLoader;
			_dayOffTemplateRepository = dayOffTemplateRepository;
			_activityRepository = activityRepository;
			_fixedStaffSchedulingService = fixedStaffSchedulingService;
			_scheduleCommand = scheduleCommand;
			_schedulerStateHolder = schedulerStateHolder;
			_requiredScheduleHelper = requiredScheduleHelper;
			_groupPagePerDateHolder = groupPagePerDateHolder;
			_scheduleTagSetter = scheduleTagSetter;
			_personSkillProvider = personSkillProvider;
			_persister = persister;
			_violatedSchedulePeriodBusinessRule = violatedSchedulePeriodBusinessRule;
		}

		[HttpPost, Route("api/ResourcePlanner/Schedule/FixedStaff"), Authorize, UnitOfWork]
		public virtual IHttpActionResult FixedStaff([FromBody]FixedStaffSchedulingInput input)
		{
			var period = new DateOnlyPeriod(new DateOnly(input.StartDate), new DateOnly(input.EndDate));

			makeSurePrereqsAreLoaded();

			var people = _fixedStaffLoader.Load(period);

			_setupStateHolderForWebScheduling.Setup(period, people);

			var allSchedules = extractAllSchedules(_schedulerStateHolder().SchedulingResultState, people, period);

			initializePersonSkillProviderBeforeAccessingItFromOtherThreads(period, people.AllPeople);
			_scheduleTagSetter().ChangeTagToSet(NullScheduleTag.Instance);
			
			var daysScheduled = 0;
			if (allSchedules.Any())
			{
				EventHandler<SchedulingServiceBaseEventArgs> schedulingServiceOnDayScheduled = (sender, args) => daysScheduled++;
				var fixedStaffSchedulingService = _fixedStaffSchedulingService();
				fixedStaffSchedulingService.DayScheduled += schedulingServiceOnDayScheduled;

				_scheduleCommand().Execute(new OptimizerOriginalPreferences(new SchedulingOptions
				{
					UseAvailability = true,
					UsePreferences = true,
					UseRotations = true,
					UseStudentAvailability = false,
					DayOffTemplate = _dayOffTemplateRepository.FindAllDayOffsSortByDescription()[0],
					ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff,
					GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.Hierarchy),
					TagToUseOnScheduling = NullScheduleTag.Instance
				}), new NoBackgroundWorker(), _schedulerStateHolder(), allSchedules, _groupPagePerDateHolder(),
					_requiredScheduleHelper(),
					new OptimizationPreferences());
				fixedStaffSchedulingService.DayScheduled -= schedulingServiceOnDayScheduled;
			}

			var conflicts = new List<PersistConflict>();
			foreach (var schedule in _schedulerStateHolder().Schedules)
			{
				conflicts.AddRange(_persister.Persist(schedule.Value));
			}
			var schedulePeriodNotInRange = _violatedSchedulePeriodBusinessRule.GetResult(people.SelectedPeople, period).ToList();
			var daysOffValidationResult = getDayOffBusinessRulesValidationResults(_schedulerStateHolder().Schedules, schedulePeriodNotInRange);
			var voilatedBusinessRules = new List<BusinessRulesValidationResult>();
			voilatedBusinessRules.AddRange(schedulePeriodNotInRange);
			voilatedBusinessRules.AddRange(daysOffValidationResult);
			return
				Ok(new SchedulingResultModel
				{
					DaysScheduled = daysScheduled,
					ConflictCount = conflicts.Count(),
					ScheduledAgentsCount = successfulScheduledAgents(_schedulerStateHolder().Schedules),
					BusinessRulesValidationResults = voilatedBusinessRules
				});
		}

		private static int successfulScheduledAgents(IEnumerable<KeyValuePair<IPerson, IScheduleRange>> schedules)
		{
			return
				schedules.Count(
					x =>
						(x.Value.CalculatedContractTimeHolder.HasValue && x.Value.CalculatedTargetTimeHolder.HasValue) &&
						(x.Value.CalculatedContractTimeHolder.Value != x.Value.CalculatedTargetTimeHolder));
		}

		private IEnumerable<BusinessRulesValidationResult> getDayOffBusinessRulesValidationResults(IEnumerable<KeyValuePair<IPerson, IScheduleRange>> schedules, List<BusinessRulesValidationResult> schedulePeriodNotInRange)
		{
			var result = new List<BusinessRulesValidationResult>();
			foreach (var item in schedules )
			{
				if(isAmongInvalidScheduleRange(schedulePeriodNotInRange,item.Key)) continue;
				var scheduleRange = item.Value;
				if(scheduleRange.CalculatedTargetScheduleDaysOff.HasValue && scheduleRange.CalculatedScheduleDaysOff.HasValue )
					if(scheduleRange.CalculatedTargetScheduleDaysOff != scheduleRange.CalculatedScheduleDaysOff)
						result.Add(new BusinessRulesValidationResult()
						{
							BusinessRuleCategory = BusinessRuleCategory.DayOff.ToString(),
							Message = string.Format(UserTexts.Resources.TargetDayOffNotFulfilledMessage, scheduleRange.CalculatedTargetScheduleDaysOff),
							Name = item.Key.Name.ToString(NameOrderOption.FirstNameLastName)
						});
			}
			return result;
		} 

		private static bool isAmongInvalidScheduleRange(List<BusinessRulesValidationResult> schedulePeriodNotInRange, IPerson person)
		{
			return schedulePeriodNotInRange.Contains(new BusinessRulesValidationResult()
			{
				BusinessRuleCategory = BusinessRuleCategory.SchedulePeriod.ToString(),
				Name = person.Name.ToString(NameOrderOption.FirstNameLastName)
			});
		}

		private static IList<IScheduleDay> extractAllSchedules(ISchedulingResultStateHolder stateHolder, PeopleSelection people,
			DateOnlyPeriod period)
		{
			var allSchedules = new List<IScheduleDay>();
			foreach (var schedule in stateHolder.Schedules)
			{
				if (people.SelectedPeople.Contains(schedule.Key))
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

		private void makeSurePrereqsAreLoaded()
		{
			_activityRepository.LoadAll();
			_dayOffTemplateRepository.LoadAll();
		}
	}
}