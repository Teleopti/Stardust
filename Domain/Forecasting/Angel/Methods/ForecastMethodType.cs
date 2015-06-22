namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public enum ForecastMethodType
	{
		None = -1,
		TeleoptiClassicLongTermWithTrend = 1,
		TeleoptiClassicShortTerm,
		TeleoptiClassicLongTermWithDayInMonth,
		TeleoptiClassicLongTerm,
		TeleoptiClassicMediumTerm,
		TeleoptiClassicMediumTermWithDayInMonth,
		TeleoptiClassicMediumTermWithTrend,
		TeleoptiClassicMediumTermWithDayInMonthWithTrend,
		TeleoptiClassicLongTermWithDayInMonthWithTrend
	}
}