namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for classes to create the exact type of <see cref="ISchedulePeriodTargetCalculator"/>.
    /// </summary>
    public interface ISchedulePeriodTargetCalculatorFactory
    {
        /// <summary>
        /// Creates this <see cref="ISchedulePeriodTargetCalculator"/>.
        /// </summary>
        /// <returns></returns>
        ISchedulePeriodTargetCalculator CreatePeriodTargetCalculator();
    }
}