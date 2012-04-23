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

		public WorkTimeMinMaxCalculator(IRuleSetProjectionService ruleSetProjectionService, IEffectiveRestrictionForDisplayCreator effectiveRestrictionCreator)
		{
			_ruleSetProjectionService = ruleSetProjectionService;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
		}

		public IWorkTimeMinMax WorkTimeMinMax(DateOnly date, IPerson person, IScheduleDay scheduleDay)
		{
			var personPeriod = person.PersonPeriods(new DateOnlyPeriod(date, date)).SingleOrDefault();
			if (personPeriod == null)
				return null;

			var ruleSetBag = personPeriod.RuleSetBag;
			if (ruleSetBag == null)
				return null;

			var options = new EffectiveRestrictionOptions(true, true);

			var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestrictionForDisplay(scheduleDay, options);

			return ruleSetBag.MinMaxWorkTime(_ruleSetProjectionService, date, effectiveRestriction);
		}
	}
}