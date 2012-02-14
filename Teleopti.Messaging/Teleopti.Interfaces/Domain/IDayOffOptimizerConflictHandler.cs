
namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Handle conflict when opitimizing day off
    /// </summary>
    public interface IDayOffOptimizerConflictHandler
    {
        /// <summary>
        /// Handle conflict
        /// </summary>
        /// <param name="dateOnly"></param>
        /// <returns></returns>
        bool HandleConflict(DateOnly dateOnly);
    }
}
