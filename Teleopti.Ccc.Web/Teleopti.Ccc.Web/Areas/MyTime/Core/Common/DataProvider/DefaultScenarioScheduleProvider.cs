using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class DefaultScenarioScheduleProvider : IScheduleProvider, IStudentAvailabilityProvider
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

		public IEnumerable<IScheduleDay> GetScheduleForPeriod(DateOnlyPeriod period)
		{
			var person = _loggedOnUser.CurrentUser();
			var defaultScenario = _scenarioRepository.Current();

			var dictionary = _scheduleRepository.FindSchedulesOnlyForGivenPeriodAndPerson(
				person,
				new ScheduleDictionaryLoadOptions(true, true), 
				period,
				defaultScenario);

			return dictionary[person].ScheduledDayCollection(period);
		}

		public IEnumerable<IScheduleDay> GetScheduleForPersons(DateOnly date, IEnumerable<IPerson> persons)
		{
			var defaultScenario = _scenarioRepository.Current();

			var dictionary = _scheduleRepository.FindSchedulesOnlyInGivenPeriod(
				new PersonProvider(persons), 
				new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(date, date),
			    defaultScenario);

			return dictionary.SchedulesForDay(date);
		}

		public IStudentAvailabilityRestriction GetStudentAvailabilityForDate(IEnumerable<IScheduleDay> scheduleDays, DateOnly date)
		{
			var studentAvailabilityDay = GetStudentAvailabilityDayForDate(scheduleDays, date);
			return studentAvailabilityDay == null ? null : GetStudentAvailabilityForDay(studentAvailabilityDay);
		}

		public IStudentAvailabilityRestriction GetStudentAvailabilityForDay(IStudentAvailabilityDay studentAvailabilityDay)
		{
			var studentAvailabilityRestrictions =
				(from sr in studentAvailabilityDay.RestrictionCollection
				select sr).ToArray();
			if (!studentAvailabilityRestrictions.Any())
				return null;
			if (studentAvailabilityRestrictions.Count() > 1)
				throw new MoreThanOneStudentAvailabilityFoundException();
			return studentAvailabilityRestrictions.Single();
		}

		public IStudentAvailabilityDay GetStudentAvailabilityDayForDate(DateOnly date)
		{
			var period = new DateOnlyPeriod(date, date);
			var scheduleDays = GetScheduleForPeriod(period);
			return GetStudentAvailabilityDayForDate(scheduleDays, date);
		}

		private IStudentAvailabilityDay GetStudentAvailabilityDayForDate(IEnumerable<IScheduleDay> scheduleDays, DateOnly date)
		{
			var studentAvailabilityDays = (from d in scheduleDays
										   where d.DateOnlyAsPeriod.DateOnly.Equals(date)
										   from sd in d.PersonRestrictionCollection().OfType<IStudentAvailabilityDay>()
										   select sd).ToList();
			if (studentAvailabilityDays.Count() > 1)
				throw new MoreThanOneStudentAvailabilityFoundException();
			if (studentAvailabilityDays.Any())
				return studentAvailabilityDays.Single();
			return null;
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
	public class MoreThanOneStudentAvailabilityFoundException : Exception
	{
	}

}