namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Enumeration for how the period value is calculated
    /// </summary>
    public enum  PeriodValueCalculationMethodOption
    {
        /// <summary>
        /// Value calculated by the daily absolut personnel deficit
        /// </summary>
        CalculateByAbsolutePersonnelDeficit,

        /// <summary>
        /// Value calculated by the daily relative personnel deficit
        /// </summary>
        CalculateByRelativePersonnelDeficit,

        /// <summary>
        /// Value calculated by absolut intraday personnel deficit
        /// </summary>
        CalculateByIntradayPersonnelDeficit
    }
}