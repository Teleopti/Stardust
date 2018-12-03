using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ViolatedSchedulePeriodBusinessRule
	{
		public IEnumerable<BusinessRulesValidationResult> GetResult(IEnumerable<IPerson> persons, DateOnlyPeriod period)
		{
			var result = new List<BusinessRulesValidationResult>();
			persons.ForEach(x =>
			{
			var schedulePeriod = x.SchedulePeriod(period.StartDate);
			if (schedulePeriod != null)
			{
				var spRange = schedulePeriod.GetSchedulePeriod(period.StartDate);
				if (!(spRange != null && period.Contains(spRange.Value)))
					result.Add(createBusinessRule(x));
			}
			else
			{
				result.Add(createBusinessRule(x));
			}
			});
			return result;
		}

		private static BusinessRulesValidationResult createBusinessRule(IPerson x)
		{
			return new BusinessRulesValidationResult
			{
				BusinessRuleCategoryText = "Schedule period",
				Message = UserTexts.Resources.SchedulePeriodNotInRange,
				Name = x.Name.ToString(NameOrderOption.FirstNameLastName)
			};
		}
	}
}