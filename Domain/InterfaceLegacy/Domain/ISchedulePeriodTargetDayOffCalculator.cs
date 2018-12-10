namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Interface for classes to calculate the period day off target.
	/// </summary>
	public interface ISchedulePeriodTargetDayOffCalculator
	{
		/// <summary>
		/// Gets the target dayoff value of the virtual schedule period.
		/// </summary>
		/// <param name="virtualSchedulePeriod"></param>
		/// <returns></returns>
		MinMax<int> TargetDaysOff(IVirtualSchedulePeriod virtualSchedulePeriod);
	}
}