namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Calculates work time for a <see cref="IWorkShift"/>.
	/// </summary>
	public interface IWorkShiftWorkTime
	{
		/// <summary>
		/// Returns a <see cref="IWorkTimeMinMax"/> based on ruleset and restriction.
		/// </summary>
		/// <param name="workShiftRuleSet"></param>
		/// <param name="restriction"></param>
		/// <returns></returns>
		IWorkTimeMinMax CalculateMinMax(IWorkShiftRuleSet workShiftRuleSet, IWorkTimeMinMaxRestriction restriction);
	}
}