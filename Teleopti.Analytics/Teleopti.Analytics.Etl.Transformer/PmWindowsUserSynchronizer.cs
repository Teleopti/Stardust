using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.PerformanceManager;
using Teleopti.Analytics.PM.PMServiceHost;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.Transformer
{
	public class PmWindowsUserSynchronizer : IPmWindowsUserSynchronizer
	{
		public IList<UserDto> Synchronize(
			IList<IPerson> users, 
			IPmPermissionTransformer transformer, 
			IPmPermissionExtractor permissionExtractor, 
			IUnitOfWorkFactory unitOfWorkFactory,
			string olapServer,
			string olapDatabase)
		{
			var windowsAuthResult = transformer.IsPmWindowsAuthenticated(olapServer, olapDatabase);
			if (!windowsAuthResult.Success)
				throw new PmSynchronizeException(windowsAuthResult.ErrorMessage);

			if (!windowsAuthResult.IsWindowsAuthentication)
				return new List<UserDto>();

			var windowsAuthUsers = transformer.GetUsersWithPermissionsToPerformanceManager(users, true, permissionExtractor, unitOfWorkFactory);
			ResultDto synchronizeUserResult = transformer.SynchronizeUserPermissions(windowsAuthUsers, olapServer, olapDatabase);

			if (!synchronizeUserResult.Success)
				throw new PmSynchronizeException(synchronizeUserResult.ErrorMessage);

			return synchronizeUserResult.ValidAnalyzerUsers;
		} 
	}

	public interface IPmWindowsUserSynchronizer
	{
		IList<UserDto> Synchronize(
			IList<IPerson> users,
			IPmPermissionTransformer transformer,
			IPmPermissionExtractor permissionExtractor,
			IUnitOfWorkFactory unitOfWorkFactory,
			string olapServer,
			string olapDatabase);
	}
}
