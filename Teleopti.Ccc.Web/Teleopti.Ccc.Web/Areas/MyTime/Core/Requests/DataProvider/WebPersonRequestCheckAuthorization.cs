using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class WebPersonRequestCheckAuthorization : IPersonRequestCheckAuthorization
	{
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

		public bool HasCancelRequestPermission (IPersonRequest personRequest)
		{
			return hasPermission (DefinedRaptorApplicationFunctionPaths.WebCancelRequest, personRequest) ||
				   hasPermission (DefinedRaptorApplicationFunctionPaths.MyTimeCancelRequest, personRequest);
		}

		private static bool hasPermission(string applicationFunctionPath, IPersonRequest personRequest)
		{
			DateOnly dateOnly = new DateOnly(personRequest.RequestedDate);
			return PrincipalAuthorization.Current().IsPermitted(applicationFunctionPath, dateOnly, personRequest.Person);
		}
	}
}