namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
	/// <summary>
	/// Different ways of loading day off data into stage table
	/// </summary>
	public enum DayOffEtlLoadSource
	{
		/// <summary>
		/// Day offs are loded from the agent preferences
		/// </summary>
		SchedulePreference,
		/// <summary>
		/// Day offs are loaded from the scheduled day offs
		/// </summary>
		ScheduleDayOff
	}
}
