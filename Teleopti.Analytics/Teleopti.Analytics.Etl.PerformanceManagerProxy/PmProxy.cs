using System.ServiceModel;
using Teleopti.Analytics.Etl.Interfaces.PerformanceManager;
using Teleopti.Analytics.PM.PMServiceHost;

namespace Teleopti.Analytics.Etl.PerformanceManagerProxy
{
	public class PmProxy : ClientBase<IPMService>, IPmProxy
	{
		public ResultDto AddUsersToSynchronize(UserDto[] users)
		{
			return Channel.AddUsersToSynchronize(users);
		}

		public ResultDto SynchronizeUsers(string olapServer, string olapDatabase)
		{
			return Channel.SynchronizeUsers(olapServer, olapDatabase);
		}

		public void ResetUserLists()
		{
			Channel.ResetUserLists();
		}

		public ResultDto IsWindowsAuthentication(string olapServer, string olapDatabase)
		{
			return Channel.IsWindowsAuthentication(olapServer, olapDatabase);
		}
	}
}
