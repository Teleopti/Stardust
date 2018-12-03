namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Class for checking Person Account Balance
    /// </summary>
    /// <remarks>
    /// Created by: HenryG
    /// Created date: 2010-04-19
    /// </remarks>
    public interface IPersonAccountBalanceCalculator
    {
        /// <summary>
        /// Checks the balance.
        /// </summary>
        /// <param name="scheduleRange">The schedule range.</param>
        /// <param name="period">The affected period</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-04-19
        /// </remarks>
        bool CheckBalance(IScheduleRange scheduleRange, DateOnlyPeriod period);
    }
}