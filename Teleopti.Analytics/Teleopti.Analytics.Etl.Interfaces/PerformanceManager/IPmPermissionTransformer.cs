using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.PM.PMServiceHost;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.Interfaces.PerformanceManager
{
	public interface IPmPermissionTransformer : IEtlTransformer<PmUser>
	{
		IList<PmUser> GetUsersWithPermissionsToPerformanceManager(IList<IPerson> personCollection, IPmPermissionExtractor permissionExtractor, IUnitOfWorkFactory unitOfWorkFactory);
		//ResultDto SynchronizeUserPermissions(IList<UserDto> users, string olapServer, string olapDatabase);
		//ResultDto IsPmWindowsAuthenticated(string olapServer, string olapDatabase);
	}
}