using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    public interface IPersonAuthorizationInfo
    {
        Guid PersonId { get; }
        Guid? TeamId { get; }
        Guid? SiteId { get; }
        Guid BusinessUnitId { get; }
    }
}