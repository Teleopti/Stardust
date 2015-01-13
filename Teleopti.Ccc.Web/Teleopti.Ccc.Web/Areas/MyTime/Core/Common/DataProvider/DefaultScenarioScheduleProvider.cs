using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class DefaultScenarioScheduleProvider : IScheduleProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly ICurrentScenario _scenarioRepository;

		public DefaultScenarioScheduleProvider(ILoggedOnUser loggedOnUser, IScheduleRepository scheduleRepository, ICurrentScenario scenarioRepository)
		{
			_loggedOnUser = loggedOnUser;
			_scheduleRepository = scheduleRepository;
			_scenarioRepository = scenarioRepository;
		}

		public IEnumerable<IScheduleDay> GetScheduleForPeriod(DateOnlyPeriod period, IScheduleDictionaryLoadOptions options = null)
		{
			options = options ?? new ScheduleDictionaryLoadOptions(true, true);

			var person = _loggedOnUser.CurrentUser();
			var defaultScenario = _scenarioRepository.Current();

			var dictionary = _scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(
				person,
				options, 
				period,
				defaultScenario);

			return dictionary[person].ScheduledDayCollection(period);
		}

		public IEnumerable<IScheduleDay> GetScheduleForPersons(DateOnly date, IEnumerable<IPerson> persons)
		{
			var defaultScenario = _scenarioRepository.Current();

			var dictionary = _scheduleRepository.FindSchedulesForPersonsOnlyInGivenPeriod(
				persons, 
				new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(date, date),
			    defaultScenario);

			return dictionary.SchedulesForDay(date);
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
	public class MoreThanOneStudentAvailabilityFoundException : Exception
	{
	}

}