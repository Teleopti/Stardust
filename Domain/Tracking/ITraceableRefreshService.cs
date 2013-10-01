using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Tracking
{

    /// <summary>
    /// Refreshes traceables (PersonAccounts) if it has not been refreshed
    /// </summary>
    public interface ITraceableRefreshService
    {
        /// <summary>
        /// Needses the refresh.
        /// </summary>
        /// <param name="traceable">The traceable.</param>
        /// <returns></returns>
        bool NeedsRefresh(ITraceable traceable);

        /// <summary>
        /// Refreshes the specified person account.
        /// </summary>
        /// <param name="personAccount">The person account.</param>
        void Refresh(IAccount account);

        /// <summary>
        /// Refreshes the specified person account if is not cached.
        /// </summary>
        /// <param name="personAccount">The person account.</param>
        void RefreshIfNeeded(IAccount account);
    }
}
