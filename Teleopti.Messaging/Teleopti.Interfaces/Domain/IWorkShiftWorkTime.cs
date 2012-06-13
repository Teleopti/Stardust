namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Calculates work time for a <see cref="IWorkShift"/>.
	/// </summary>
	public interface IWorkShiftWorkTime
	{
		IWorkTimeMinMax CalculateMinMax(IWorkShiftRuleSet workShiftRuleSet,
		                                                IEffectiveRestriction effectiveRestriction);
	}
}