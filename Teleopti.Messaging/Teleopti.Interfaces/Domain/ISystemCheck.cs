namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Checks if a part of a system works as expected
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2009-12-23
    /// </remarks>
    public interface ISystemCheck
    {
        /// <summary>
        /// Determines whether [is running ok].
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is running ok]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-12-23
        /// </remarks>
        bool IsRunningOk();

        /// <summary>
        /// Gets the warning text.
        /// </summary>
        /// <value>The warning text.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-12-23
        /// </remarks>
        string WarningText { get; }
    }
}
