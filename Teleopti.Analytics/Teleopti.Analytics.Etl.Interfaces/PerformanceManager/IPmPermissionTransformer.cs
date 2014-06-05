using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.PM.PMServiceHost;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Interfaces.PerformanceManager
{
    public interface IPmPermissionTransformer : IEtlTransformer<UserDto>
    {
        IList<UserDto> GetUsersWithPermissionsToPerformanceManager(IList<IPerson> personCollection, bool filterWindowsLogOn, IPmPermissionExtractor permissionExtractor, IUnitOfWorkFactory unitOfWorkFactory);
        ResultDto SynchronizeUserPermissions(IList<UserDto> users, string olapServer, string olapDatabase);
        IList<UserDto> GetPmUsersForAllBusinessUnits(string jobStepName, IList<IJobResult> jobResultCollection, IList<UserDto> usersForCurrentBusinessUnit);
    }
}