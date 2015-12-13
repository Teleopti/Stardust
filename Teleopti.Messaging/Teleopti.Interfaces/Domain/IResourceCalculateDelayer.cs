namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Used to decide if it is time to do a resource calculation
	/// </summary>
	public interface IResourceCalculateDelayer
	{
		bool CalculateIfNeeded(DateOnly scheduleDateOnly, DateTimePeriod? workShiftProjectionPeriod);
		void Pause();
		void Resume();
	}
}