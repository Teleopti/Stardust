using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping
{
	public class StudentAvailabilityDomainData
	{
		private readonly IScheduleProvider _scheduleProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private IEnumerable<IScheduleDay> _scheduleDays;

		public StudentAvailabilityDomainData(IScheduleProvider scheduleProvider, ILoggedOnUser loggedOnUser)
		{
			_scheduleProvider = scheduleProvider;
			_loggedOnUser = loggedOnUser;
		}

		public DateOnlyPeriod Period;
		public DateOnly ChoosenDate;
		public IPerson Person { get { return _loggedOnUser.CurrentUser(); } }

		public DateOnlyPeriod DisplayedPeriod
		{
			get
			{
				var startDate = DateHelper.GetFirstDateInWeek(Period.StartDate, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek).AddDays(-7);
				var endDate = DateHelper.GetFirstDateInWeek(Period.EndDate, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek).AddDays(6).AddDays(7);
				return new DateOnlyPeriod(startDate, endDate);
			}
		}

		public IEnumerable<IScheduleDay> ScheduleDays
		{
			get
			{
				return _scheduleDays ?? (_scheduleDays = _scheduleProvider.GetScheduleForPeriod(DisplayedPeriod));
			} 
			set
			{
				_scheduleDays = value;
			}
		}
	}

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

		//this one should probably go away - need to stop refactoring on release branch though...
		public DateOnlyPeriod DisplayedPeriod
		{
			get
			{
				var startDate = DateHelper.GetFirstDateInWeek(Period.StartDate, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek).AddDays(-7);
				var endDate = DateHelper.GetFirstDateInWeek(Period.EndDate, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek).AddDays(6).AddDays(7);
				return new DateOnlyPeriod(startDate, endDate);
			}
		}
	}
}