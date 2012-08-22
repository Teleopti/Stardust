using System;
using System.Globalization;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class SchedulePeriod : IUserSetup
	{
		public int VirtualSchedulePeriodWeeks;
		public Domain.Scheduling.Assignment.SchedulePeriod TheSchedulePeriod;
		public int CreatedWeeksAgo;

		private CultureInfo _cultureInfo;
		private IPerson _person;

		public SchedulePeriod() : this(0, 2) { }
		public SchedulePeriod(int createdWeeksAgo) : this(createdWeeksAgo, 2) { }
		public SchedulePeriod(int createdWeeksAgo, int virtualSchedulePeriodWeeks)
		{
			CreatedWeeksAgo = createdWeeksAgo;
			VirtualSchedulePeriodWeeks = virtualSchedulePeriodWeeks;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			_person = user;
			_cultureInfo = cultureInfo;

			var createdDate = DateHelper.GetFirstDateInWeek(DateTime.Now.Date, cultureInfo);
			createdDate = createdDate.AddDays(CreatedWeeksAgo * 7 * -1);
			TheSchedulePeriod = new Domain.Scheduling.Assignment.SchedulePeriod(
				new DateOnly(createdDate),
				SchedulePeriodType.Week,
				VirtualSchedulePeriodWeeks);
			user.AddSchedulePeriod(TheSchedulePeriod);
		}

		public DateTime FirstDateInVirtualSchedulePeriod() { return DateHelper.GetFirstDateInWeek(DateTime.Now.Date, _cultureInfo); }
		public DateTime LastDateInVirtualSchedulePeriod() { return FirstDateInVirtualSchedulePeriod().AddDays(7 * VirtualSchedulePeriodWeeks).AddDays(-1); }

		public DateTime FirstDayOfDisplayedPeriod() { return DateHelper.GetFirstDateInWeek(FirstDateInVirtualSchedulePeriod(), _cultureInfo).AddDays(-7); }
		public DateTime LastDayOfDisplayedPeriod() { return DateHelper.GetLastDateInWeek(LastDateInVirtualSchedulePeriod(), _cultureInfo).AddDays(7); }



		public DateTime FirstDayOfNextDisplayedVirtualSchedulePeriod() { return FirstDayOfDisplayedPeriod().AddDays(7 * VirtualSchedulePeriodWeeks); }
		public DateTime LastDayOfNextDisplayedVirtualSchedulePeriod() { return LastDayOfDisplayedPeriod().AddDays(7 * VirtualSchedulePeriodWeeks); }

		public DateTime FirstDayOfDisplayedPreviousVirtualSchedulePeriod() { return FirstDayOfDisplayedPeriod().AddDays(-1 * 7 * VirtualSchedulePeriodWeeks); }
		public DateTime LastDayOfDisplayedPreviousVirtualSchedulePeriod() { return LastDayOfDisplayedPeriod().AddDays(-1 * 7 * VirtualSchedulePeriodWeeks); }



		public IVirtualSchedulePeriod VirtualSchedulePeriodForDate(DateOnly date)
		{
			return _person.VirtualSchedulePeriod(date);
		}

		public DateOnlyPeriod DisplayedVirtualSchedulePeriodForDate(DateOnly date)
		{
			var virtualSchedulePeriod = VirtualSchedulePeriodForDate(date);
			var startDate = virtualSchedulePeriod.DateOnlyPeriod.StartDate;
			var endDate = virtualSchedulePeriod.DateOnlyPeriod.EndDate;
			var displayedStartDate = DateHelper.GetFirstDateInWeek(startDate, _cultureInfo).AddDays(-7);
			var displayedEndDate = DateHelper.GetLastDateInWeek(endDate, _cultureInfo).AddDays(7);
			return new DateOnlyPeriod(new DateOnly(displayedStartDate), new DateOnly(displayedEndDate));
		}
	}
}