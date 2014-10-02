using System;

namespace Teleopti.Ccc.Rta.Server
{
    public interface IRtaDataHandlerCache
    {
        void InvalidateReadModelCache(Guid personId);
    }
}