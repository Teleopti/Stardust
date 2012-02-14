namespace Teleopti.Interfaces.Domain
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
    }
}