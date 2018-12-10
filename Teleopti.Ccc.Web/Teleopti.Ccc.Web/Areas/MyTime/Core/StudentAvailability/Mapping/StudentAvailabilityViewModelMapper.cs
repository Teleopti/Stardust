using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping
{
	public class StudentAvailabilityViewModelMapper
	{
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IStudentAvailabilityProvider _studentAvailabilityProvider;
		private readonly IVirtualSchedulePeriodProvider _virtualSchedulePeriodProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly INow _now;

		public StudentAvailabilityViewModelMapper(
			IScheduleProvider scheduleProvider,
			IStudentAvailabilityProvider studentAvailabilityProvider,
			IVirtualSchedulePeriodProvider virtualSchedulePeriodProvider,
			ILoggedOnUser loggedOnUser,
			INow now)
		{
			_scheduleProvider = scheduleProvider;
			_studentAvailabilityProvider = studentAvailabilityProvider;
			_virtualSchedulePeriodProvider = virtualSchedulePeriodProvider;
			_loggedOnUser = loggedOnUser;
			_now = now;
		}

		public StudentAvailabilityViewModel Map(DateOnly s)
		{
			var domainData = new StudentAvailabilityDomainData(_scheduleProvider, _loggedOnUser)
			{
				ChoosenDate = s,
				Period = _virtualSchedulePeriodProvider.GetCurrentOrNextVirtualPeriodForDate(s)
			};
			var periodSelection = new PeriodSelectionViewModel
			{
				Date = s.ToFixedClientDateOnlyFormat(),
				Display = domainData.Period.DateString,
				StartDate = domainData.Period.StartDate.Date,
				EndDate = domainData.Period.EndDate.Date,
				PeriodNavigation = new PeriodNavigationViewModel
				{
					CanPickPeriod = true,
					HasNextPeriod = true,
					HasPrevPeriod = true,
					NextPeriod = domainData.Period.EndDate.AddDays(1).ToFixedClientDateOnlyFormat(),
					PrevPeriod = domainData.Period.StartDate.AddDays(-1).ToFixedClientDateOnlyFormat()
				},
				SelectableDateRange = new PeriodDateRangeViewModel
				{
					MinDate = DateOnly.MinValue.ToFixedClientDateOnlyFormat(),
					MaxDate = DateOnly.MaxValue.ToFixedClientDateOnlyFormat()
				},
				SelectedDateRange = new PeriodDateRangeViewModel
				{
					MinDate = domainData.Period.StartDate.ToFixedClientDateOnlyFormat(),
					MaxDate = domainData.Period.EndDate.ToFixedClientDateOnlyFormat()
				}
			};

			return new StudentAvailabilityViewModel
			{
				PeriodSelection = periodSelection,
				PeriodSummary = new PeriodSummaryViewModel(),
				Styles = new StyleClassViewModel[] {},
				WeekDayHeaders =
					DateHelper.GetWeekdayNames(CultureInfo.CurrentCulture).Select(n => new WeekDayHeader {Title = n}).ToList(),
				Weeks = weeks(domainData),
				StudentAvailabilityPeriod = map(domainData.Person.WorkflowControlSet)
			};
		}

		private StudentAvailabilityPeriodViewModel map(IWorkflowControlSet s)
		{
			if (s == null) return null;
			return new StudentAvailabilityPeriodViewModel
			{
				Period = s.StudentAvailabilityPeriod.ToString(),
				OpenPeriod = s.StudentAvailabilityInputPeriod.ToString()
			};
		}

		private IList<WeekViewModel> weeks(StudentAvailabilityDomainData s)
		{
			var firstDatesOfWeeks = new List<DateOnly>();
			var firstDateOfWeek = s.DisplayedPeriod.StartDate;
			while (firstDateOfWeek < s.DisplayedPeriod.EndDate)
			{
				firstDatesOfWeeks.Add(firstDateOfWeek);
				firstDateOfWeek = firstDateOfWeek.AddDays(7);
			}
			var mappingDatas = firstDatesOfWeeks.Select(d =>
					new StudentAvailabilityWeekDomainData(d, s.Person, s.Period, s.ScheduleDays));
			return mappingDatas.Select(map).ToArray();
		}

		private WeekViewModel map(StudentAvailabilityWeekDomainData s)
		{
			var dates = s.FirstDateOfWeek.DateRange(7);
			var dateOnlys = dates.Select(d => d);
			var mappingDatas = dateOnlys.Select(d =>
					new StudentAvailabilityDayDomainData(d, s.Period, s.Person, _studentAvailabilityProvider, s.ScheduleDays));

			return new WeekViewModel
			{
				Summary = new WeekSummaryViewModel(),
				Days = mappingDatas.Select(map).ToArray()
			};
		}

		private AvailableDayViewModel map(StudentAvailabilityDayDomainData s)
		{
			return new AvailableDayViewModel
			{
				Date = s.Date,
				Header = new HeaderViewModel
				{
					DayDescription = dayDescription(s),
					DayNumber = s.Date.Day.ToString()
				},
				Editable = editable(s),
				InPeriod = s.Period.Contains(s.Date)
			};
		}

		private bool editable(StudentAvailabilityDayDomainData s)
		{
			if (s.Person.WorkflowControlSet == null) return false;
			var insideSchedulePeriod = s.Period.Contains(s.Date);
			var insideInputPeriod = s.Person.WorkflowControlSet.StudentAvailabilityInputPeriod.Contains(_now.ServerDate_DontUse());
			var insideStudentAvailabilityPeriod = s.Person.WorkflowControlSet.StudentAvailabilityPeriod.Contains(s.Date);
			return insideSchedulePeriod && insideInputPeriod && insideStudentAvailabilityPeriod;
		}

		private static string dayDescription(StudentAvailabilityDayDomainData s)
		{
			if (s.Date.Day.Equals(1))
				return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(s.Date.Month);
			if (s.Date.Equals(s.DisplayedPeriod.StartDate))
				return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(s.Date.Month);
			return string.Empty;
		}
	}
}