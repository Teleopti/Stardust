using System;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class ForecastedStaffingViewModelCreator
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly ForecastedStaffingProvider _forecastedStaffingProvider;

		public ForecastedStaffingViewModelCreator(
			INow now,
			IUserTimeZone timeZone,
			ForecastedStaffingProvider forecastedStaffingProvider)
		{
			_now = now;
			_timeZone = timeZone;
			_forecastedStaffingProvider = forecastedStaffingProvider;
		}

		public IntradayStaffingViewModel Load(Guid[] skillIdList)
		{
			var usersToday = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone()));

			var staffingIntervals = _forecastedStaffingProvider.Load(skillIdList, usersToday);
			
			staffingIntervals = staffingIntervals
				.GroupBy(g => g.StartTime)
				.Select(s => new StaffingIntervalModel
				{
					StartTime = TimeZoneHelper.ConvertFromUtc(s.First().StartTime, _timeZone.TimeZone()),
					Agents = s.Sum(a => a.Agents)
				}).ToList();

			var staffingForUsersToday = staffingIntervals
												.Where(t => t.StartTime >= usersToday.Date && t.StartTime < usersToday.Date.AddDays(1))
												.ToArray();
			return new IntradayStaffingViewModel()
			{
				DataSeries = new StaffingDataSeries()
				{
					Time = staffingForUsersToday
								.Select(t => t.StartTime)
								.ToArray(),
					ForecastedStaffing = staffingForUsersToday
								.Select(t => t.Agents)
								.ToArray()
				}
			};
		}
	}
}