namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// 
	/// </summary>
	public interface IForecastScheduleValuePair
	{
		/// <summary>
		/// Gets or sets the forecast value.
		/// </summary>
		/// <value>
		/// The forecast value.
		/// </value>
		double ForecastValue { get; set; }
		/// <summary>
		/// Gets or sets the schedule value.
		/// </summary>
		/// <value>
		/// The schedule value.
		/// </value>
		double ScheduleValue { get; set; }

		bool BreaksMinimumAgents { get; set; }
	}
}