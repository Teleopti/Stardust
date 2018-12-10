using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class DefaultScenarioForStudentAvailabilityScheduleProvider : IStudentAvailabilityProvider
	{
		private readonly IScheduleProvider _scheduleProvider;

		public DefaultScenarioForStudentAvailabilityScheduleProvider(IScheduleProvider scheduleProvider)
		{
			_scheduleProvider = scheduleProvider;
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
			var scheduleDays = _scheduleProvider.GetScheduleForPeriod(period);
			return GetStudentAvailabilityDayForDate(scheduleDays, date);
		}

		public IEnumerable<IStudentAvailabilityDay> GetStudentAvailabilityDayForPeriod(DateOnlyPeriod period)
		{
			var scheduleDays = _scheduleProvider.GetScheduleForStudentAvailability(period);

			return scheduleDays.Select(s =>
				(s.PersonRestrictionCollection() == null ||
				 !s.PersonRestrictionCollection().OfType<IStudentAvailabilityDay>().Any())
					? new StudentAvailabilityDay(s.Person, s.DateOnlyAsPeriod.DateOnly, new List<IStudentAvailabilityRestriction>())
					: s.PersonRestrictionCollection().OfType<IStudentAvailabilityDay>().FirstOrDefault());
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
}