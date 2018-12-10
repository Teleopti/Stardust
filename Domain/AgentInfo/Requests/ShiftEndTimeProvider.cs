using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class ShiftEndTimeProvider : IShiftEndTimeProvider
	{
		private readonly ICurrentScenario _scenarioRepository;
		private readonly IScheduleStorage _scheduleStorage;

		public ShiftEndTimeProvider(IScheduleStorage scheduleStorage, ICurrentScenario scenarioRepository)
		{
			_scheduleStorage = scheduleStorage;
			_scenarioRepository = scenarioRepository;
		}

		public DateTime? GetShiftEndTimeForPerson(IPerson person, DateOnly date)
		{
			var dictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
				new[] { person },
				new ScheduleDictionaryLoadOptions(false, false),
				date.ToDateOnlyPeriod(),
				_scenarioRepository.Current());

			var personAssignment = dictionary[person].ScheduledDay(date).PersonAssignment();

			if(personAssignment != null && (personAssignment.MainActivities().Any() || personAssignment.OvertimeActivities().Any()))
				return personAssignment.Period.EndDateTime;

			return null;
		}
	}
}
