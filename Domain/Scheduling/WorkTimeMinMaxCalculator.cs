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

		public WorkTimeMinMaxCalculator(IRuleSetProjectionService ruleSetProjectionService)
		{
			_ruleSetProjectionService = ruleSetProjectionService;
		}

		public IWorkTimeMinMax WorkTimeMinMax(IPerson person, DateOnly date)
		{
			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
																			  new WorkTimeLimitation(), null, null, null,
																			  new List<IActivityRestriction>());
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