using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class SchedulingValidator
	{
		private readonly IMissingForecastProvider _missingForecastProvider;
		private readonly IPersonSkillValidator _personSkillValidator;
		private readonly IPersonPeriodValidator _personPeriodValidator;
		private readonly IPersonSchedulePeriodValidator _personSchedulePeriodValidator;
		private readonly IPersonShiftBagValidator _personShiftBagValidator;
		private readonly IPersonPartTimePercentageValidator _partTimePercentageValidator;
		private readonly IPersonContractValidator _personContractValidator;
		private readonly IPersonContractScheduleValidator _personContractScheduleValidator;

		public SchedulingValidator(IMissingForecastProvider missingForecastProvider, IPersonSkillValidator personSkillValidator, IPersonPeriodValidator personPeriodValidator, IPersonSchedulePeriodValidator personSchedulePeriodValidator, IPersonShiftBagValidator personShiftBagValidator, IPersonPartTimePercentageValidator partTimePercentageValidator, IPersonContractValidator personContractValidator, IPersonContractScheduleValidator personContractScheduleValidator)
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