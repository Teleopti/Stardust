
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Handle conflict when opitimizing day off
    /// </summary>
    public interface IDayOffOptimizerConflictHandler
    {
        /// <summary>
        /// Handle conflict
        /// </summary>
        /// <param name="schedulingOptions">The scheduling options.</param>
        /// <param name="dateOnly">The date only.</param>
        /// <returns></returns>
        bool HandleConflict(SchedulingOptions schedulingOptions, DateOnly dateOnly);
    }
}
