using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.Interfaces.PerformanceManager
{
    public interface IPmPermissionExtractor
    {
        PmPermissionType ExtractPermission(ICollection<IApplicationFunction> applicationFunctionCollection, IUnitOfWorkFactory unitOfWorkFactory);
    }
}