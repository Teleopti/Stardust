using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class SchedulingValidator
	{
		private readonly MissingForecastProvider _missingForecastProvider;
		private readonly PersonSkillValidator _personSkillValidator;
		private readonly PersonPeriodValidator _personPeriodValidator;
		private readonly PersonSchedulePeriodValidator _personSchedulePeriodValidator;
		private readonly PersonShiftBagValidator _personShiftBagValidator;
		private readonly PersonPartTimePercentageValidator _partTimePercentageValidator;
		private readonly PersonContractValidator _personContractValidator;
		private readonly PersonContractScheduleValidator _personContractScheduleValidator;

		public SchedulingValidator(MissingForecastProvider missingForecastProvider, 
			PersonSkillValidator personSkillValidator, 
			PersonPeriodValidator personPeriodValidator, 
			PersonSchedulePeriodValidator personSchedulePeriodValidator, 
			PersonShiftBagValidator personShiftBagValidator, 
			PersonPartTimePercentageValidator partTimePercentageValidator, 
			PersonContractValidator personContractValidator, 
			PersonContractScheduleValidator personContractScheduleValidator)
		{
			_missingForecastProvider = missingForecastProvider;
			_personSkillValidator = personSkillValidator;
			_personPeriodValidator = personPeriodValidator;
			_personSchedulePeriodValidator = personSchedulePeriodValidator;
			_personShiftBagValidator = personShiftBagValidator;
			_partTimePercentageValidator = partTimePercentageValidator;
			_personContractValidator = personContractValidator;
			_personContractScheduleValidator = personContractScheduleValidator;
		}

		public ValidationResult Validate(ValidationParameters parameters, IEnumerable<SkillMissingForecast> existingForecast)
		{
			var result = new ValidationResult();
			_missingForecastProvider.GetMissingForecast(parameters.People, parameters.Period, existingForecast)
				.ForEach(r => result.Add(r));
			_personPeriodValidator.GetPeopleMissingPeriod(parameters.People, parameters.Period)
				.ForEach(r => result.Add(r));
			_personSkillValidator.GetPeopleMissingSkill(parameters.People, parameters.Period)
				.ForEach(r => result.Add(r));
			_personSchedulePeriodValidator.GetPeopleMissingSchedulePeriod(parameters.People, parameters.Period)
				.ForEach(r => result.Add(r));
			_personShiftBagValidator.GetPeopleMissingShiftBag(parameters.People, parameters.Period)
				.ForEach(r => result.Add(r));
			_partTimePercentageValidator.GetPeopleMissingPartTimePercentage(parameters.People, parameters.Period)
				.ForEach(r => result.Add(r));
			_personContractValidator.GetPeopleMissingContract(parameters.People, parameters.Period)
				.ForEach(r => result.Add(r));
			_personContractScheduleValidator.GetPeopleMissingContractSchedule(parameters.People, parameters.Period)
				.ForEach(r => result.Add(r));
			return result;
		}
	}
}