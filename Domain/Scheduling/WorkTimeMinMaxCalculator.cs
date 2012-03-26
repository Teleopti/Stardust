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

		public IWorkTimeMinMax WorkTimeMinMax(IPerson person, DateOnly date, IScenario scenario)
		{
			var dateTime = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);

			if (person == null)
				throw new ArgumentNullException("person");

			var personPeriod = person.PersonPeriods(new DateOnlyPeriod(date, date)).SingleOrDefault();
			if (personPeriod == null)
				throw new InvalidOperationException("No person period defined");

			var ruleSetBag = personPeriod.RuleSetBag;
			if (ruleSetBag == null)
				throw new InvalidOperationException("No rule set bag defined");

			var options = new EffectiveRestrictionOptions(true);
			var scheduleDictionary = _scheduleRepository.FindSchedulesOnlyInGivenPeriod(new PersonProvider(new[] {person}),
			                                                                     new ScheduleDictionaryLoadOptions(true, false),
			                                                                     new DateTimePeriod(dateTime, dateTime), scenario);
			var scheduleDay = scheduleDictionary[person].ScheduledDay(date);
			IEffectiveRestriction effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestrictionForDisplay(scheduleDay, options);

			return ruleSetBag.MinMaxWorkTime(_ruleSetProjectionService, date, effectiveRestriction);
		}
	}
}