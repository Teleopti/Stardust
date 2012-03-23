using System;
using System.Collections.Generic;
using System.Linq;
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

		public IWorkTimeMinMax WorkTimeMinMax(IPerson person, DateOnly date)
		{
			IEffectiveRestriction effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestrictionForDisplay();
			var personPeriod = person.PersonPeriods(new DateOnlyPeriod(date, date)).SingleOrDefault();
			if (personPeriod == null)
				throw new InvalidOperationException("No person period defined");

			var ruleSetBag = personPeriod.RuleSetBag;
			if (ruleSetBag == null)
				throw new InvalidOperationException("No rule set bag defined");

			return ruleSetBag.MinMaxWorkTime(_ruleSetProjectionService, date, effectiveRestriction);
		}
	}
}