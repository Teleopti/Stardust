using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public class SkillForecastReadModelPeriodBuilder
	{
		private INow _now;
		private readonly SkillForecastSettingsReader _skillForecastSettingsReader;
		private readonly IStaffingSettingsReader _staffingSettingsReader;

		public SkillForecastReadModelPeriodBuilder(INow now, SkillForecastSettingsReader skillForecastSettingsReader, IStaffingSettingsReader staffingSettingsReader)
		{
			_now = now;
			_skillForecastSettingsReader = skillForecastSettingsReader;
			_staffingSettingsReader = staffingSettingsReader;
		}

		public DateTimePeriod BuildFullPeriod()
		{
			var startDate = _now.UtcDateTime().Date.AddDays(-_skillForecastSettingsReader.NumberOfDaysInPast);
			var endDate = _now.UtcDateTime().Date.AddDays(_staffingSettingsReader.GetIntSetting("StaffingReadModelNumberOfDays", 49));
			endDate = endDate.AddDays(_skillForecastSettingsReader.NumberOfExtraDaysInFuture);
			return new DateTimePeriod(startDate.Date,endDate.Date);
		}

		public DateTimePeriod BuildNextPeriod(DateTime lastRun)
		{
			var staffingDaysNum = _staffingSettingsReader.GetIntSetting("StaffingReadModelNumberOfDays", 49);
			var extraDaysForForecast = _skillForecastSettingsReader.NumberOfExtraDaysInFuture;
			var startDate = lastRun.AddDays(staffingDaysNum + extraDaysForForecast);
			var endDate = startDate.AddDays(extraDaysForForecast);
			return new DateTimePeriod(startDate.Date,endDate.Date);
		}
	}

	public class SkillForecastSettingsReader
	{
		public int NumberOfDaysInPast { get; }
		public int NumberOfExtraDaysInFuture { get; }
		public int NumberOfDaysForNextJobRun { get; }
		public int MaximumEstimatedExecutionTimeOfJobInMinutes { get; }

		public SkillForecastSettingsReader()
		{
			NumberOfDaysInPast = 8;
			NumberOfExtraDaysInFuture = 9;
			NumberOfDaysForNextJobRun = 7;
			MaximumEstimatedExecutionTimeOfJobInMinutes = 60;
		}
	}
}