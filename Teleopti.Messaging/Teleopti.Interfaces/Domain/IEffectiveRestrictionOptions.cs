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
	}
}