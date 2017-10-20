using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Tracking
{
    public interface ITraceableRefreshService
    {
        bool NeedsRefresh(ITraceable traceable);

        void Refresh(IAccount account);

        void RefreshIfNeeded(IAccount account);
    }
}
