namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Use permissions?
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2009-12-07
    /// </remarks>
    public interface IPermissionCheck
    {
        /// <summary>
        /// Turn off/on permission checks.
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-12-07
        /// </remarks>
        void UsePermissions(bool enabled);

        /// <summary>
        /// Gets the synchronization object.
        /// Should expose an object to hold locks on, 
        /// to turn off/on permissions in a thread safe way
        /// </summary>
        /// <value>The synchronization object.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-12-07
        /// </remarks>
        object SynchronizationObject { get; }
    }
}
