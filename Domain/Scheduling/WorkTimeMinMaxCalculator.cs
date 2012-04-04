using System;
using System.Collections.Generic;
using System.Linq;
using Domain;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class WorkTimeMinMaxCalculator : IWorkTimeMinMaxCalculator
	{
		private readonly IRuleSetProjectionService _ruleSetProjectionService;
		private readonly IEffectiveRestrictionForDisplayCreator _effectiveRestrictionCreator;
		private readonly IScheduleRepository _scheduleRepository;

		public WorkTimeMinMaxCalculator(IRuleSetProjectionService ruleSetProjectionService, IEffectiveRestrictionForDisplayCreator effectiveRestrictionCreator, IScheduleRepository scheduleRepository)
		{
			_ruleSetProjectionService = ruleSetProjectionService;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_scheduleRepository = scheduleRepository;
		}

		public IWorkTimeMinMax WorkTimeMinMax(IPerson person, DateOnly scheduleDate, IScenario scenario)
		{
			var dateTime = new DateTime(scheduleDate.Year, scheduleDate.Month, scheduleDate.Day, 0, 0, 0, DateTimeKind.Utc);

			if (person == null)
				throw new ArgumentNullException("person");

			var personPeriod = person.PersonPeriods(new DateOnlyPeriod(scheduleDate, scheduleDate)).SingleOrDefault();
			if (personPeriod == null)
				return null;

			var ruleSetBag = personPeriod.RuleSetBag;
			if (ruleSetBag == null)
				return null;

			var options = new EffectiveRestrictionOptions(true, true);
			var scheduleDictionary = _scheduleRepository.FindSchedulesOnlyInGivenPeriod(new PersonProvider(new[] {person}),
			                                                                     new ScheduleDictionaryLoadOptions(true, false),
			                                                                     new DateTimePeriod(dateTime, dateTime), scenario);
			var scheduleDay = scheduleDictionary[person].ScheduledDay(scheduleDate);
			IEffectiveRestriction effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestrictionForDisplay(scheduleDay, options);

			return ruleSetBag.MinMaxWorkTime(_ruleSetProjectionService, scheduleDate, effectiveRestriction);
		}
	}
}