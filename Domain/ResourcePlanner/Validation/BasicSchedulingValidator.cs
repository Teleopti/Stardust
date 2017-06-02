using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class BasicSchedulingValidator
	{
		private readonly SchedulingValidator _schedulingValidator;
		private readonly IExistingForecastRepository _existingForecastRepository;
		private readonly IScenarioRepository _scenarioRepository;

		public BasicSchedulingValidator(SchedulingValidator schedulingValidator, 
				IExistingForecastRepository existingForecastRepository,
				IScenarioRepository scenarioRepository)
		{
			_schedulingValidator = schedulingValidator;
			_existingForecastRepository = existingForecastRepository;
			_scenarioRepository = scenarioRepository;
		}

		public ValidationResult Validate(ValidationParameters parameters)
		{
			var scenario = _scenarioRepository.LoadDefaultScenario();
			var existingForecast = _existingForecastRepository.ExistingForecastForAllSkills(parameters.Period, scenario);

			return _schedulingValidator.Validate(parameters, existingForecast);
		}
	}
}