using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Analytics.Etl.Common.Interfaces.PerformanceManager
{
	public interface IPmPermissionTransformer : IEtlTransformer<PmUser>
	{
		IList<PmUser> GetUsersWithPermissionsToPerformanceManager(IList<IPerson> personCollection, IPmPermissionExtractor permissionExtractor, IUnitOfWorkFactory unitOfWorkFactory);
	}
}