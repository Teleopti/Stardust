namespace Teleopti.Interfaces.Domain
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
