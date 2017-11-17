using System.Web;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.Provider;

namespace Teleopti.Ccc.Web.Areas.SSO.Core
{
	public class CustomErrorReporting : IErrorReporting
	{
		public string LogError(ProtocolException exception)
		{
			throw new HttpException(404, exception.Message);
		}

		public string Contact { get; private set; }
	}
}