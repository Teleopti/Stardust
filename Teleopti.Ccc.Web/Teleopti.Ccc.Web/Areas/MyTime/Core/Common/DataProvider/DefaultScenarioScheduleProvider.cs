using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class DefaultScenarioScheduleProvider : IScheduleProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _scenarioRepository;

		public DefaultScenarioScheduleProvider(ILoggedOnUser loggedOnUser, IScheduleStorage scheduleStorage,
			ICurrentScenario scenarioRepository)
		{
			_loggedOnUser = loggedOnUser;
			_scheduleStorage = scheduleStorage;
			_scenarioRepository = scenarioRepository;
		}

		public IEnumerable<IScheduleDay> GetScheduleForPeriod(DateOnlyPeriod period, ScheduleDictionaryLoadOptions options = null)
		{
			return getSchedule(period, options);
		}

		/// <summary>
		/// This method will ignore published date and view unpublished schdule permission setting to get 
		/// student availability after published date even if current user has no permission to view unpublished schedule.
		/// It should only be used to get schedule to retrieve student availability.
		/// Refer to bug #33327: Agents can no longer see Availability they entered for dates that have not been published.
		/// </summary>
		public IEnumerable<IScheduleDay> GetScheduleForStudentAvailability(DateOnlyPeriod period, ScheduleDictionaryLoadOptions options = null)
		{
			return getSchedule(period, options, ScheduleVisibleReasons.StudentAvailability);
		}
		
		public IEnumerable<IScheduleDay> GetScheduleForPersons(DateOnly date, IEnumerable<IPerson> persons, bool loadNotes = false)
		{
			var options = new ScheduleDictionaryLoadOptions(false, loadNotes);

			var defaultScenario = _scenarioRepository.Current();
			var dictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
				persons,
				options,
				date.ToDateOnlyPeriod(),
				defaultScenario);

			return persons.Select(p => dictionary[p].ScheduledDay(date));
		}

		public IEnumerable<IScheduleDay> GetScheduleForPersonsInPeriod(DateOnlyPeriod period, IEnumerable<IPerson> persons,
			ScheduleDictionaryLoadOptions options = null)
		{
			if (options == null)
			{
				options = new ScheduleDictionaryLoadOptions(false, false);
			}

			var defaultScenario = _scenarioRepository.Current();

			var people = persons.ToArray();
			var dictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
				people,
				options,
				period,
				defaultScenario);
			return dictionary.SchedulesForPeriod(period, people);
		}

		private IEnumerable<IScheduleDay> getSchedule(DateOnlyPeriod period,
			ScheduleDictionaryLoadOptions options = null,
			ScheduleVisibleReasons visibleReason = ScheduleVisibleReasons.Published)
		{
			options = options ?? new ScheduleDictionaryLoadOptions(true, true);

			var person = _loggedOnUser.CurrentUser();
			var defaultScenario = _scenarioRepository.Current();

			var dictionary = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
				person,
				options,
				period,
				defaultScenario);

			var scheduleRange = dictionary[person];
			return visibleReason.HasFlag(ScheduleVisibleReasons.StudentAvailability)
				? scheduleRange.ScheduledDayCollectionNoViewPublishedScheduleCheck(period)
				: scheduleRange.ScheduledDayCollection(period);
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
	public class MoreThanOneStudentAvailabilityFoundException : Exception
	{
	}
}