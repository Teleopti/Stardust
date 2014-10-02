using System;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
    public interface IRtaDataHandlerCache
    {
        void InvalidateReadModelCache(Guid personId);
    }
}