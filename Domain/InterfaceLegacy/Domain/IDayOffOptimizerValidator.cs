using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Validator for optimizing day offs
    /// </summary>
    public interface IDayOffOptimizerValidator
    {
        /// <summary>
        /// Validate
        /// </summary>
        /// <returns></returns>
        bool Validate(DateOnly dateOnly, IScheduleMatrixPro scheduleMatrixPro);
    }
}
