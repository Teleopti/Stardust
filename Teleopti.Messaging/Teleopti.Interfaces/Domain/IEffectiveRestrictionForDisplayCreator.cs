namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Interface for effective restriction for display reator
	/// </summary>
	public interface IEffectiveRestrictionForDisplayCreator
	{
		/// <summary>
		/// Creates the effective restriction
		/// </summary>
		/// <returns></returns>
		IEffectiveRestriction GetEffectiveRestrictionForDisplay(IScheduleDay scheduleDay, IEffectiveRestrictionOptions effectiveRestrictionOptions);
	}
}