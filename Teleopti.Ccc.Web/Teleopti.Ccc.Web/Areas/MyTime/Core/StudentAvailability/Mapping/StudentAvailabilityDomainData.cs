using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

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
				var startDate = DateHelper.GetFirstDateInWeek(Period.StartDate, CultureInfo.CurrentCulture).AddDays(-7);
				var endDate = DateHelper.GetLastDateInWeek(Period.EndDate, CultureInfo.CurrentCulture).AddDays(7);
				return new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));
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
}