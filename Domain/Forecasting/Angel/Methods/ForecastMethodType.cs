namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public enum ForecastMethodType
	{
		None = -1,
		TeleoptiClassicShortTerm = 1,
		TeleoptiClassicMediumTerm = 2,
		TeleoptiClassicMediumTermWithDayInMonth = 3,
		TeleoptiClassicLongTerm = 4,
		TeleoptiClassicLongTermWithDayInMonth = 5,
		TeleoptiClassicMediumTermWithTrend = 12,
		TeleoptiClassicMediumTermWithDayInMonthWithTrend = 13,
		TeleoptiClassicLongTermWithTrend = 14,
		TeleoptiClassicLongTermWithDayInMonthWithTrend = 15
	}
}