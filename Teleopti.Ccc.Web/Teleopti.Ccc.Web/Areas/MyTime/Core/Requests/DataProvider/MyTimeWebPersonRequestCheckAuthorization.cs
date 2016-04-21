using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class MyTimeWebPersonRequestCheckAuthorization : IPersonRequestCheckAuthorization
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
			return hasPermission(DefinedRaptorApplicationFunctionPaths.WebCancelRequest, personRequest);
		}

		private static bool hasPermission(string applicationFunctionPath, IPersonRequest personRequest)
		{
			DateOnly dateOnly = new DateOnly(personRequest.RequestedDate);
			return PrincipalAuthorization.Instance().IsPermitted(applicationFunctionPath, dateOnly, personRequest.Person);
		}
	}
}