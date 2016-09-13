using System;
using System.Data;
using System.Globalization;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsDateRepositoryWithCreation : AnalyticsDateRepositoryBase, IAnalyticsDateRepository
	{
		private readonly IDistributedLockAcquirer _distributedLockAcquirer;
		private readonly IEventPublisher _eventPublisher;

		public AnalyticsDateRepositoryWithCreation(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork, IDistributedLockAcquirer distributedLockAcquirer, IEventPublisher eventPublisher) : base(analyticsUnitOfWork)
		{
			_distributedLockAcquirer = distributedLockAcquirer;
			_eventPublisher = eventPublisher;
		}

		public new IAnalyticsDate MaxDate()
		{
			return AnalyticsDate.Eternity;
		}

		public new IAnalyticsDate Date(DateTime dateDate)
		{
			return base.Date(dateDate) ?? createDatesTo(dateDate);
		}

		private IAnalyticsDate createDatesTo(DateTime dateDate)
		{
			using (_distributedLockAcquirer.LockForTypeOf(new dimDateCreationLock()))
			{
				// Check again to see that we still don't have the date after aquiring the lock
				using (var session = AnalyticsUnitOfWork.Current().Session().SessionFactory.OpenSession())
				using (var transaction = session.BeginTransaction(IsolationLevel.ReadUncommitted))
				{
					var date = Date(session, dateDate);
					transaction.Commit();
					if (date != null)
						return date;
				}
				
				var currentMax = base.MaxDate() ?? new AnalyticsDate { DateDate = new DateTime(1999, 12, 30) };
				var currentDay = currentMax.DateDate;

				using (var session = AnalyticsUnitOfWork.Current().Session().SessionFactory.OpenStatelessSession())
				using (var transaction = session.BeginTransaction())
				{
					while ((currentDay += TimeSpan.FromDays(1)) <= dateDate)
					{
						session.Insert(new AnalyticsDate
						{
							DateDate = currentDay,
							Year = currentDay.Year,
							YearMonth = int.Parse($"{currentDay.Year}{currentDay.Month.ToString("D2")}"),
							Month = currentDay.Month,
							MonthName = DateHelper.GetMonthName(currentDay, CultureInfo.CurrentCulture),
							MonthResourceKey = currentDay.Month.GetMonthResourceKey(),
							DayInMonth = currentDay.Day,
							WeekNumber = DateHelper.WeekNumber(currentDay, CultureInfo.CurrentCulture),
							WeekdayName = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)currentDay.DayOfWeek],
							WeekdayResourceKey = currentDay.GetDayOfWeek().GetWeekDayResourceKey(),
							WeekdayNumber = currentDay.GetDayOfWeek(),
							YearWeek = currentDay.GetYearWeek(),
							Quarter = currentDay.GetQuarter(),
							InsertDate = DateTime.UtcNow
						});
					}
					transaction.Commit();
					_eventPublisher.Publish(new AnalyticsDatesChangedEvent());
					return base.Date(dateDate);
				}
			}
		}

		private class dimDateCreationLock
		{

		}
	}

	public static class Extensions
	{
		public static string GetMonthResourceKey(this int month)
		{
			switch (month)
			{
				case 1:
					return "ResMonthJanuary";
				case 2:
					return "ResMonthFebruary";
				case 3:
					return "ResMonthMarch";
				case 4:
					return "ResMonthApril";
				case 5:
					return "ResMonthMay";
				case 6:
					return "ResMonthJune";
				case 7:
					return "ResMonthJuly";
				case 8:
					return "ResMonthAugust";
				case 9:
					return "ResMonthSeptember";
				case 10:
					return "ResMonthOctober";
				case 11:
					return "ResMonthNovember";
				case 12:
					return "ResMonthDecember";
				default:
					return "";
			}
		}

		public static string GetWeekDayResourceKey(this DayOfWeek weekDayNumber)
		{
			var number = (int)weekDayNumber;
			if (weekDayNumber == DayOfWeek.Sunday)
				number = 7;
			return number.GetWeekDayResourceKey();
		}

		public static string GetWeekDayResourceKey(this int weekDayNumber)
		{
			switch (weekDayNumber)
			{
				case 1:
					return "ResDayOfWeekMonday";
				case 2:
					return "ResDayOfWeekTuesday";
				case 3:
					return "ResDayOfWeekWednesday";
				case 4:
					return "ResDayOfWeekThursday";
				case 5:
					return "ResDayOfWeekFriday";
				case 6:
					return "ResDayOfWeekSaturday";
				case 7:
					return "ResDayOfWeekSunday";
				default:
					return "";
			}
		}

		public static int GetDayOfWeek(this DateTime date)
		{
			//In data mart Sunday = 0 and Saturday = 6.
			var ret = (int)date.DayOfWeek;
			if (ret == 0) ret = 7;
			return ret;
		}

		public static string GetYearWeek(this DateTime date)
		{
			var weekNumber = DateHelper.WeekNumber(date, CultureInfo.CurrentCulture);
			var datePart = weekNumber.ToString(CultureInfo.InvariantCulture);
			if (datePart.Length < 2)
				datePart = string.Concat("0", datePart);

			var year = date.Year;
			if (date.Day <= 6 && weekNumber > 51)
				year -= 1;
			if (date.Day >= 26 && weekNumber == 1)
				year += 1;

			return string.Format(CultureInfo.InvariantCulture, "{0}{1}", year, datePart);
		}

		public static string GetQuarter(this DateTime date) { return string.Concat(date.Year.ToString(CultureInfo.InvariantCulture), "Q", DateHelper.GetQuarter(date.Month)); }
	}
}