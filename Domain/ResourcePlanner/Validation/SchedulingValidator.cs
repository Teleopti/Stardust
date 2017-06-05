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

		public ValidationResult Validate(IEnumerable<IPerson> agents, DateOnlyPeriod period, IEnumerable<SkillMissingForecast> existingForecast, bool desktop)
		{
			var result = new ValidationResult();
			_personPeriodValidator.FillPeopleMissingPeriod(result, agents, period);
			_personSkillValidator.FillPeopleMissingSkill(result, agents, period);
			if (desktop)
			{
				_missingForecastValidator.FillMissingForecast(result, agents, period, existingForecast);
				_personSchedulePeriodValidator.FillPeopleMissingSchedulePeriod(result, agents, period);
			}
			_personShiftBagValidator.FillPeopleMissingShiftBag(result, agents, period);
			_partTimePercentageValidator.FillPeopleMissingPartTimePercentage(result, agents, period);
			_personContractValidator.FillPeopleMissingContract(result, agents, period);
			_personContractScheduleValidator.FillPeopleMissingContractSchedule(result, agents, period);
			return result;
		}
	}
}