using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

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

		public ValidationResult Validate(IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			var scenario = _scenarioRepository.LoadDefaultScenario();
			var existingForecast = _existingForecastRepository.ExistingForecastForAllSkills(period, scenario) ?? Enumerable.Empty<SkillMissingForecast>();

			return _schedulingValidator.Validate(agents, period, existingForecast);
		}
	}
}