using System;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class SchedulePeriod : IUserSetup
	{
		public int VirtualSchedulePeriodWeeks;
		public Domain.Scheduling.Assignment.SchedulePeriod TheSchedulePeriod;
		public int CreatedWeeksAgo;
		public DateTime Date = new DateTime(2001,1,1);

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

			var createdDate = DateHelper.GetFirstDateInWeek(Date, cultureInfo);
			createdDate = createdDate.AddDays(CreatedWeeksAgo * 7 * -1);
			TheSchedulePeriod = new Domain.Scheduling.Assignment.SchedulePeriod(
				new DateOnly(createdDate),
				SchedulePeriodType.Week,
				VirtualSchedulePeriodWeeks);

			user.RemoveAllSchedulePeriods();
			user.AddSchedulePeriod(TheSchedulePeriod);
		}

		public DateTime FirstDateInVirtualSchedulePeriod() { return DateHelper.GetFirstDateInWeek(Date, _cultureInfo); }
		
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