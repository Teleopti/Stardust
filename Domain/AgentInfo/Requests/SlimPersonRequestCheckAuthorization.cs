using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class SlimPersonRequestCheckAuthorization : IPersonRequestCheckAuthorization
	{
		private readonly ICurrentAuthorization _currentAuthorization;

		public SlimPersonRequestCheckAuthorization(ICurrentAuthorization currentAuthorization)
		{
			_currentAuthorization = currentAuthorization;
		}

		public void VerifyEditRequestPermission(IPersonRequest personRequest)
		{
		}

		public bool HasEditRequestPermission(IPersonRequest personRequest)
		{
			return true;
		}

		public bool HasViewRequestPermission(IPersonRequest personRequest)
		{
			return true;
		}

		public bool HasCancelRequestPermission(IPersonRequest personRequest)
		{
			return hasPermission(DefinedRaptorApplicationFunctionPaths.WebCancelRequest, personRequest) ||
				   hasPermission(DefinedRaptorApplicationFunctionPaths.MyTimeCancelRequest, personRequest);
		}

		private bool hasPermission(string applicationFunctionPath, IPersonRequest personRequest)
		{
			DateOnly dateOnly = new DateOnly(personRequest.RequestedDate);
			return _currentAuthorization.Current().IsPermitted(applicationFunctionPath, dateOnly, personRequest.Person);
		}
	}
}