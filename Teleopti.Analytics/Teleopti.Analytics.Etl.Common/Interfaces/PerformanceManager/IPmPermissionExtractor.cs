﻿using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.Common.Interfaces.PerformanceManager
{
    public interface IPmPermissionExtractor
    {
        PmPermissionType ExtractPermission(ICollection<IApplicationFunction> applicationFunctionCollection, IUnitOfWorkFactory unitOfWorkFactory);
    }
}