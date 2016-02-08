using System;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance
{
	public interface IScheduleGenerator
	{
		void Generate(Guid personId, DateOnly date);
	}

	public class ScheduleGenerator : IScheduleGenerator
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IShiftCategoryRepository _shiftCategoryRepository;
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IActivityRepository _activityRepository;

		public ScheduleGenerator(ICurrentUnitOfWork unitOfWork,
			IScenarioRepository scenarioRepository,
			IPersonRepository personRepository,
			IScheduleStorage scheduleStorage,
			IShiftCategoryRepository shiftCategoryRepository,
			IPersonAbsenceAccountRepository personAbsenceAccountRepository,
			IScheduleDifferenceSaver scheduleDifferenceSaver,
			ISchedulingResultStateHolder schedulingResultStateHolder, 
			IActivityRepository activityRepository
			)
		{
			_unitOfWork = unitOfWork;
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_scheduleStorage = scheduleStorage;
			_shiftCategoryRepository = shiftCategoryRepository;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_activityRepository = activityRepository;
		}

		public void Generate(Guid personId, DateOnly date)
		{
			var person = _personRepository.Load(personId);
			var scenario = _scenarioRepository.LoadDefaultScenario();
			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
				person, new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(date, date.AddDays(1)), scenario);

			var scheduleRange = scheduleDictionary[person];

			_schedulingResultStateHolder.Schedules = scheduleRange.Owner;
			_schedulingResultStateHolder.PersonsInOrganization = new Collection<IPerson> { scheduleRange.Person };
			_schedulingResultStateHolder.AllPersonAccounts = _personAbsenceAccountRepository.FindByUsers(_schedulingResultStateHolder.PersonsInOrganization);
			var rules = NewBusinessRuleCollection.MinimumAndPersonAccount(_schedulingResultStateHolder);
			rules.Remove(typeof(NewPersonAccountRule));
			
			((IValidateScheduleRange)scheduleRange).ValidateBusinessRules(rules);

			var scheduleDay = scheduleRange.ScheduledDay(date);
			var shiftCategory = _shiftCategoryRepository.LoadAll().First();
			var defaultActivity = _activityRepository.LoadAll().First();
			var currentAss = scheduleDay.PersonAssignment();
			if (currentAss == null)
			{
				currentAss = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, scheduleDay.DateOnlyAsPeriod.DateOnly);
				scheduleDay.Add(currentAss);
			}
			currentAss.SetShiftCategory(shiftCategory);
			currentAss.AddActivity(defaultActivity,
				new DateTimePeriod(date.Year, date.Month, date.Day, 8, date.Year, date.Month, date.Day, 17));
			
			var dic = (IReadOnlyScheduleDictionary)scheduleDay.Owner;
			dic.MakeEditable();

			var invalidList = dic.Modify(ScheduleModifier.Scheduler,
				scheduleDay, rules, new ResourceCalculationOnlyScheduleDayChangeCallback(),
				new ScheduleTagSetter(NullScheduleTag.Instance));
			if (invalidList != null && invalidList.Any(l => !l.Overridden))
			{
				throw new Exception(
					string.Format(System.Globalization.CultureInfo.InvariantCulture,
						"At least one business rule was broken. Messages are: {0}{1}", Environment.NewLine,
						string.Join(Environment.NewLine,
							invalidList.Select(i => i.Message).Distinct().ToArray())));
			}

			_scheduleDifferenceSaver.SaveChanges(dic.DifferenceSinceSnapshot(), (IUnvalidatedScheduleRangeUpdate)dic[scheduleDay.Person]);
			_personAbsenceAccountRepository.AddRange(dic.ModifiedPersonAccounts);

			_unitOfWork.Current().PersistAll();
		}
	}
}