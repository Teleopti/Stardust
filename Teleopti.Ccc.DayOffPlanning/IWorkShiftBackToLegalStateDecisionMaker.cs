using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning
{
    public interface IWorkShiftBackToLegalStateDecisionMaker
    {
        /// <summary>
        /// Executes the calculation on the specified lockable bit array.
        /// </summary>
        /// <param name="lockableBitArray">The lockable bit array.</param>
        /// <param name="raise">if set to <c>true</c> [raise].</param>
        /// <returns></returns>
        int? Execute(ILockableBitArray lockableBitArray,  bool raise);
    }
}