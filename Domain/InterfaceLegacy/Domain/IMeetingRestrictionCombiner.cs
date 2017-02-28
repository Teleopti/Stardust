namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// add effectiveRestriction to scheduleDay
	/// </summary>
	public interface IMeetingRestrictionCombiner
	{
		/// <summary>
		/// add effectiveRestriction to  scheduleDay
		/// </summary>
		/// <param name="scheduleDay"> the scheduleDay</param>
		/// <param name="effectiveRestriction"> the effectiveRestriction</param>
		/// <returns>the effectiveRestriction</returns>
		IEffectiveRestriction Combine(IScheduleDay scheduleDay, IEffectiveRestriction effectiveRestriction);
	}
}