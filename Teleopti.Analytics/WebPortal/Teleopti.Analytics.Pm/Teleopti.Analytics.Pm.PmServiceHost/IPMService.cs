using System.ServiceModel;

namespace Teleopti.Analytics.PM.PMServiceHost
{
	[ServiceContract]
	public interface IPMService
	{
		[OperationContract]
		[ImpersonationValidation]
		ResultDto AddUsersToSynchronize(UserDto[] users);

		[OperationContract]
		[ImpersonationValidation]
		ResultDto SynchronizeUsers(string olapServer, string olapDatabase);

		[OperationContract]
		[ImpersonationValidation]
		void ResetUserLists();

		[OperationContract]
		[ImpersonationValidation]
		ResultDto IsWindowsAuthentication(string olapServer, string olapDatabase);
	}
}
