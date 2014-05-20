using System.Net;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.Provider;

namespace Teleopti.Ccc.Web.WindowsIdentityProvider.Core
{
	public class CustomErrorReporting : IErrorReporting
	{
		public string LogError(ProtocolException exception)
		{
			throw new WebException("Authentication error, review configuration and make sure it is correct.", exception);
		}

		public string Contact { get; private set; }
	}
}