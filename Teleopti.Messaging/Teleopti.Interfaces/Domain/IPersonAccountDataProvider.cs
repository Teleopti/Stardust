namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Provides
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2009-02-26
    /// </remarks>
    public interface IPersonAccountDataProvider
    {
        /// <summary>
        /// Updates the person account.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="loadedSchedule">The loaded schedule.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-02-26
        /// </remarks>
        void CalculatePersonAccount(IAccount account,ISchedule loadedSchedule);
    }
}
