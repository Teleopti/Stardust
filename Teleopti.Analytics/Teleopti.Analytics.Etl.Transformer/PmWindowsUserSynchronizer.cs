using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.PerformanceManager;
using Teleopti.Analytics.PM.PMServiceHost;

namespace Teleopti.Analytics.Etl.Transformer
{
	public class PmWindowsUserSynchronizer : IPmWindowsUserSynchronizer
	{
		public IList<UserDto> Synchronize(
			IList<UserDto> windowsAuthUsers, 
			IPmPermissionTransformer transformer, 
			string olapServer,
			string olapDatabase)
		{
			ResultDto synchronizeUserResult = transformer.SynchronizeUserPermissions(windowsAuthUsers, olapServer, olapDatabase);

		if (!synchronizeUserResult.Success)
				throw new PmSynchronizeException(synchronizeUserResult.ErrorMessage);

			return synchronizeUserResult.ValidAnalyzerUsers;
		} 
	}

	public interface IPmWindowsUserSynchronizer
	{
		IList<UserDto> Synchronize(
			IList<UserDto> users,
			IPmPermissionTransformer transformer,
			string olapServer,
			string olapDatabase);
	}
}
