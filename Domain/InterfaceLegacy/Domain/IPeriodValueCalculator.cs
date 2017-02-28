namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Calculates the period value.
    /// </summary>
    public interface IPeriodValueCalculator
    {
        /// <summary>
        /// Calculates the period value.
        /// </summary>
        /// <returns></returns>
        double PeriodValue(IterationOperationOption iterationOperationOption);
    }
}