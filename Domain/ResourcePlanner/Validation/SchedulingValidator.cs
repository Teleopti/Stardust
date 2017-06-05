using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class SchedulingValidator
	{
		private readonly MissingForecastValidator _missingForecastValidator;
		private readonly PersonSkillValidator _personSkillValidator;
		private readonly PersonPeriodValidator _personPeriodValidator;
		private readonly PersonSchedulePeriodValidator _personSchedulePeriodValidator;
		private readonly PersonShiftBagValidator _personShiftBagValidator;
		private readonly PersonPartTimePercentageValidator _partTimePercentageValidator;
		private readonly PersonContractValidator _personContractValidator;
		private readonly PersonContractScheduleValidator _personContractScheduleValidator;

		public SchedulingValidator(MissingForecastValidator missingForecastValidator, 
			PersonSkillValidator personSkillValidator, 
			PersonPeriodValidator personPeriodValidator, 
			PersonSchedulePeriodValidator personSchedulePeriodValidator, 
			PersonShiftBagValidator personShiftBagValidator, 
			PersonPartTimePercentageValidator partTimePercentageValidator, 
			PersonContractValidator personContractValidator, 
			PersonContractScheduleValidator personContractScheduleValidator)
		{
			_missingForecastValidator = missingForecastValidator;
			_personSkillValidator = personSkillValidator;
			_personPeriodValidator = personPeriodValidator;
			_personSchedulePeriodValidator = personSchedulePeriodValidator;
			_personShiftBagValidator = personShiftBagValidator;
			_partTimePercentageValidator = partTimePercentageValidator;
			_personContractValidator = personContractValidator;
			_personContractScheduleValidator = personContractScheduleValidator;
		}

		public ValidationResult Validate(IEnumerable<IPerson> agents, DateOnlyPeriod period, bool desktop)
		{
			var result = new ValidationResult();
			_personPeriodValidator.FillResult(result, agents, period);
			_personSkillValidator.FillResult(result, agents, period);
			if (desktop)
			{
				_missingForecastValidator.FillResult(result, agents, period);
				_personSchedulePeriodValidator.FillResult(result, agents, period);
			}
			_personShiftBagValidator.FillResult(result, agents, period);
			_partTimePercentageValidator.FillResult(result, agents, period);
			_personContractValidator.FillResult(result, agents, period);
			_personContractScheduleValidator.FillResult(result, agents, period);
			return result;
		}
	}
}