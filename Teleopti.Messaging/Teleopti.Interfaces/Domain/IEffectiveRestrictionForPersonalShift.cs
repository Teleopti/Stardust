namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// add effectiveRestriction to scheduleDay
	/// </summary>
	public interface IEffectiveRestrictionForPersonalShift
	{
		/// <summary>
		/// add effectiveRestriction to  scheduleDay
		/// </summary>
		/// <param name="scheduleDay"> the scheduleDay</param>
		/// <param name="effectiveRestriction"> the effectiveRestriction</param>
		/// <returns>the effectiveRestriction</returns>
		IEffectiveRestriction AddEffectiveRestriction(IScheduleDay scheduleDay, IEffectiveRestriction effectiveRestriction);
	}
}