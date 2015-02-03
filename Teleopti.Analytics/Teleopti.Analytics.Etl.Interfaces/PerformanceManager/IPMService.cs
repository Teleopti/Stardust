using System.ServiceModel;
using Teleopti.Analytics.PM.PMServiceHost;

namespace Teleopti.Analytics.Etl.Interfaces.PerformanceManager
{
	[ServiceContract]
	public interface IPMService
	{
		[OperationContract]
		ResultDto AddUsersToSynchronize(UserDto[] users);

		[OperationContract]
		ResultDto SynchronizeUsers(string olapServer, string olapDatabase);

		[OperationContract]
		void ResetUserLists();

		[OperationContract]
		ResultDto IsWindowsAuthentication(string olapServer, string olapDatabase);
	}
}