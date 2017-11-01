using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourcePlanner.Validation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class FullSchedulingResult
	{
		private readonly IFindSchedulesForPersons _findSchedulesForPersons;
		private readonly ICurrentScenario _currentScenario;
		private readonly IUserTimeZone _userTimeZone;
		private readonly SchedulingValidator _schedulingValidator;
		private readonly SuccessfulScheduledAgents _successfulScheduledAgents;

		public FullSchedulingResult(IFindSchedulesForPersons findSchedulesForPersons, 
			ICurrentScenario currentScenario, IUserTimeZone userTimeZone, SchedulingValidator schedulingValidator,
			SuccessfulScheduledAgents successfulScheduledAgents)
		{
			_findSchedulesForPersons = findSchedulesForPersons;
			_currentScenario = currentScenario;
			_userTimeZone = userTimeZone;
			_schedulingValidator = schedulingValidator;
			_successfulScheduledAgents = successfulScheduledAgents;
		}

		public SchedulingResultModel Execute(DateOnlyPeriod period, IEnumerable<IPerson> fixedStaffPeople)
		{
			var personsProvider = new PersonProvider(fixedStaffPeople) { DoLoadByPerson = true };
			var scheduleOfSelectedPeople = _findSchedulesForPersons.FindSchedulesForPersons(_currentScenario.Current(), personsProvider, new ScheduleDictionaryLoadOptions(false, false, false), period.ToDateTimePeriod(_userTimeZone.TimeZone()), fixedStaffPeople, true);

			return new SchedulingResultModel
			{
				ScheduledAgentsCount = _successfulScheduledAgents.Execute(scheduleOfSelectedPeople, period),
				BusinessRulesValidationResults = _schedulingValidator.Validate(new FullValidationInput(scheduleOfSelectedPeople, fixedStaffPeople, period)).InvalidResources
			};
		}
	}
}