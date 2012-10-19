namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Interface for EffectiveRestrictionOptions
	/// </summary>
	public interface IEffectiveRestrictionOptions
	{
		/// <summary>
		/// If prefereces should be considered
		/// </summary>
		bool UsePreference { get; set; }

		/// <summary>
		/// If availability should be considered
		/// </summary>
		bool UseAvailability { get; set; }

		/// <summary>
		/// If meetings should be considered
		/// </summary>
		bool UseMeetings { get; }

		/// <summary>
		/// If personal shifts should be considered
		/// </summary>
		bool UsePersonalShifts { get; }
	}
}