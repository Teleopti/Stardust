using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class DefaultScenarioScheduleProvider : IScheduleProvider, IStudentAvailabilityProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IScenarioProvider _scenarioProvider;
		private readonly IUserTimeZone _userTimeZone;

		public DefaultScenarioScheduleProvider(ILoggedOnUser loggedOnUser, IScheduleRepository scheduleRepository, IScenarioProvider scenarioProvider, IUserTimeZone userTimeZone)
		{
			_loggedOnUser = loggedOnUser;
			_scheduleRepository = scheduleRepository;
			_scenarioProvider = scenarioProvider;
			_userTimeZone = userTimeZone;
		}

		public IEnumerable<IScheduleDay> GetScheduleForPeriod(DateOnlyPeriod period)
		{
			var person = _loggedOnUser.CurrentUser();
			var defaultScenario = _scenarioProvider.DefaultScenario();
			var dateTimePeriod = period.ToDateTimePeriod(_userTimeZone.TimeZone());

			var dictionary = _scheduleRepository.FindSchedulesOnlyInGivenPeriod(new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(true, true), dateTimePeriod,
																		defaultScenario);

			return dictionary[person].ScheduledDayCollection(period);
		}

		public IEnumerable<IScheduleDay> GetScheduleForPersons(DateOnly date, IEnumerable<IPerson> persons)
		{
			var person = _loggedOnUser.CurrentUser();
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var dateTimePeriod = new DateOnlyPeriod(date, date).ToDateTimePeriod(timeZone);

			var defaultScenario = _scenarioProvider.DefaultScenario();

			var dictionary = _scheduleRepository.FindSchedulesOnlyInGivenPeriod(
				new PersonProvider(persons), 
				new ScheduleDictionaryLoadOptions(false, false), 
				dateTimePeriod,
			    defaultScenario);

			return dictionary.SchedulesForDay(date);
		}

		public IStudentAvailabilityRestriction GetStudentAvailabilityForDate(IEnumerable<IScheduleDay> scheduleDays, DateOnly date)
		{
			var studentAvailabilityDays = (from d in scheduleDays
													where d.DateOnlyAsPeriod.DateOnly.Equals(date)
													from sd in d.PersonRestrictionCollection().OfType<IStudentAvailabilityDay>()
													select sd).ToList();
			if (studentAvailabilityDays.Count() > 1)
				throw new MoreThanOneStudentAvailabilityFoundException();
			if (studentAvailabilityDays.Any())
				return GetStudentAvailabilityForDay(studentAvailabilityDays.Single());
			return null;
		}

		public IStudentAvailabilityRestriction GetStudentAvailabilityForDate(DateOnly date)
		{
			var period = new DateOnlyPeriod(date, date);
			var scheduleDays = GetScheduleForPeriod(period);
			return GetStudentAvailabilityForDate(scheduleDays, date);
		}

		public IStudentAvailabilityRestriction GetStudentAvailabilityForDay(IStudentAvailabilityDay studentAvailabilityDay)
		{
			var studentAvailabilityRestrictions =
				from sr in studentAvailabilityDay.RestrictionCollection
				select sr;
			if (!studentAvailabilityRestrictions.Any())
				return null;
			if (studentAvailabilityRestrictions.Count() > 1)
				throw new MoreThanOneStudentAvailabilityFoundException();
			return studentAvailabilityRestrictions.Single();
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
	public class MoreThanOneStudentAvailabilityFoundException : Exception
	{
	}

}