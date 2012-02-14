using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{

    /// <summary>
    /// Decides and perform bitarray move(s) on days off
    /// </summary>
    public interface IDayOffDecisionMaker
    {
        /// <summary>
        /// Excecutes the specified lockable bit array.
        /// </summary>
        /// <param name="lockableBitArray">The lockable bit array.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        bool Execute(ILockableBitArray lockableBitArray, IList<double?> values);
    }
}