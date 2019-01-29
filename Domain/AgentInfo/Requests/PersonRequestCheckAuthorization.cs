using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class PersonRequestCheckAuthorization : IPersonRequestCheckAuthorization
	{
		public void VerifyEditRequestPermission(IPersonRequest personRequest)
		{
			if (!HasEditRequestPermission(personRequest))
			{
				throw new PermissionException("You do not have sufficient privilege to run this command.");
			}
		}

		public bool HasEditRequestPermission(IPersonRequest personRequest)
		{
			return hasPermission(DefinedRaptorApplicationFunctionPaths.RequestSchedulerApprove, personRequest);
		}

		public bool HasViewRequestPermission(IPersonRequest personRequest)
		{
			return hasPermission(DefinedRaptorApplicationFunctionPaths.RequestScheduler, personRequest);
		}

		public bool HasCancelRequestPermission(IPersonRequest personRequest)
		{
			return hasPermission(DefinedRaptorApplicationFunctionPaths.WebCancelRequest, personRequest);
		}

		private static bool hasPermission(string applicationFunctionPath, IPersonRequest personRequest)
		{
			DateOnly dateOnly = new DateOnly(personRequest.RequestedDate);
			return PrincipalAuthorization.Current_DONTUSE().IsPermitted(applicationFunctionPath, dateOnly, personRequest.Person);
		}
	}
}