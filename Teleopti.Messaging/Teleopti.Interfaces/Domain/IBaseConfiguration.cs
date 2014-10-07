namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Store base configuration of ETL tool
	/// </summary>
	public interface IBaseConfiguration
	{
		/// <summary>
		/// Culture to be used in ETL Tool
		/// </summary>
		int? CultureId { get; }

		/// <summary>
		/// Interval Length to be used in ETL Tool
		/// </summary>
		int? IntervalLength { get; }

		/// <summary>
		/// Time zone to be used when converting user input to UTC
		/// </summary>
		string TimeZoneCode { get; }

		IEtlToggleManager EtlToggleManager { get; }
	}
}