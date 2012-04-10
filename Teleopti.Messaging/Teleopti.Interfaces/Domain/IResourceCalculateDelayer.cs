namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Used to decide if it is time to do a resource calculation
	/// </summary>
	public interface IResourceCalculateDelayer
	{
		/// <summary>
		/// Calculates if needed.
		/// </summary>
		/// <param name="scheduleDateOnly">The schedule date only.</param>
		/// <param name="workShiftProjectionPeriod">The work shift projection period.</param>
		/// <returns></returns>
		bool CalculateIfNeeded(DateOnly scheduleDateOnly, DateTimePeriod? workShiftProjectionPeriod);
	}
}