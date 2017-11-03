using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class FullSchedulingResult
	{
		private readonly IFindSchedulesForPersons _findSchedulesForPersons;
		private readonly ICurrentScenario _currentScenario;
		private readonly IUserTimeZone _userTimeZone;
		private readonly CheckScheduleHints _checkScheduleHints;
		private readonly SuccessfulScheduledAgents _successfulScheduledAgents;

		public FullSchedulingResult(IFindSchedulesForPersons findSchedulesForPersons, 
			ICurrentScenario currentScenario, IUserTimeZone userTimeZone, CheckScheduleHints checkScheduleHints,
			SuccessfulScheduledAgents successfulScheduledAgents)
		{
			_findSchedulesForPersons = findSchedulesForPersons;
			_currentScenario = currentScenario;
			_userTimeZone = userTimeZone;
			_checkScheduleHints = checkScheduleHints;
			_successfulScheduledAgents = successfulScheduledAgents;
		}

		public SchedulingResultModel Execute(DateOnlyPeriod period, IEnumerable<IPerson> fixedStaffPeople)
		{
			var personsProvider = new PersonProvider(fixedStaffPeople) { DoLoadByPerson = true };
			var scheduleOfSelectedPeople = _findSchedulesForPersons.FindSchedulesForPersons(_currentScenario.Current(), personsProvider, new ScheduleDictionaryLoadOptions(false, false, false), period.ToDateTimePeriod(_userTimeZone.TimeZone()), fixedStaffPeople, true);

			return new SchedulingResultModel
			{
				ScheduledAgentsCount = _successfulScheduledAgents.Execute(scheduleOfSelectedPeople, period),
				BusinessRulesValidationResults = _checkScheduleHints.Execute(new HintInput(scheduleOfSelectedPeople, fixedStaffPeople, period)).InvalidResources
			};
		}
	}
}