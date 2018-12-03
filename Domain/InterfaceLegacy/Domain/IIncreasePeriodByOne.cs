namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Inrease the period
    /// </summary>
    public interface IIncreasePeriodByOne
    {
        /// <summary>
        /// Inrease
        /// </summary>
        /// <param name="currentStartDate"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        DateOnly Increase(DateOnly currentStartDate, int number);

    	/// <summary>
    	/// Try to find a good initial starting date to avoid big loops.
    	/// </summary>
    	/// <param name="currentStartDate">The current start date for the period.</param>
    	/// <param name="number">The number to add for each round.</param>
    	/// <param name="requestedDate">The requested date.</param>
    	/// <returns></returns>
    	DateOnly EvaluateProperInitialStartDate(DateOnly currentStartDate, int number, DateOnly requestedDate);
    }
}