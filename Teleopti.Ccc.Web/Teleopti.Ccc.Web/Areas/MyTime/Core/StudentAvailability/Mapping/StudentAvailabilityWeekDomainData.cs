using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping
{
	public class StudentAvailabilityWeekDomainData
	{
		public StudentAvailabilityWeekDomainData(DateOnly firstDateOfWeek, IPerson person, DateOnlyPeriod period, IEnumerable<IScheduleDay> scheduleDays)
		{
			FirstDateOfWeek = firstDateOfWeek;
			Person = person;
			Period = period;
			ScheduleDays = scheduleDays;
		}

		public DateOnly FirstDateOfWeek { get; private set; }
		public IPerson Person { get; private set; }
		public DateOnlyPeriod Period { get; private set; }
		public IEnumerable<IScheduleDay> ScheduleDays { get; private set; }
	}


	public class StudentAvailabilityDayDomainData
	{
		public StudentAvailabilityDayDomainData(DateOnly date, DateOnlyPeriod period, IPerson person, IStudentAvailabilityProvider studentAvailabilityProvider, IEnumerable<IScheduleDay> scheduleDays)
		{
			_studentAvailabilityProvider = studentAvailabilityProvider;
			Date = date;
			Period = period;
			Person = person;
			ScheduleDays = scheduleDays;
		}

		public DateOnly Date { get; private set; }
		public DateOnlyPeriod Period { get; private set; }
		public IPerson Person { get; private set; }
		public IEnumerable<IScheduleDay> ScheduleDays { get; private set; }

		private IStudentAvailabilityRestriction _studentAvailabilityRestriction;
		private readonly IStudentAvailabilityProvider _studentAvailabilityProvider;

		public IStudentAvailabilityRestriction StudentAvailability
		{
			get
			{
				return _studentAvailabilityRestriction ?? (_studentAvailabilityRestriction = _studentAvailabilityProvider.GetStudentAvailabilityForDate(ScheduleDays, Date));
			}
		}

		public DateOnlyPeriod DisplayedPeriod
		{
			get
			{
				var startDate = DateHelper.GetFirstDateInWeek(Period.StartDate, CultureInfo.CurrentCulture).AddDays(-7);
				var endDate = DateHelper.GetLastDateInWeek(Period.EndDate, CultureInfo.CurrentCulture).AddDays(7);
				return new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));
			}
		}

	}
}