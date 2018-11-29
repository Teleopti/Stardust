using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    public interface IWorkShiftBackToLegalStateDecisionMaker
    {
		/// <summary>
		/// Executes the calculation on the specified lockable bit array.
		/// </summary>
		/// <param name="lockableBitArray">The lockable bit array.</param>
		/// <param name="raise">if set to <c>true</c> [raise].</param>
		/// <param name="?">The ?.</param>
		/// <param name="period">The period.</param>
		/// <returns></returns>
        int? Execute(ILockableBitArray lockableBitArray,  bool raise, DateOnlyPeriod period);
    }
}