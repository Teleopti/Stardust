using System.Threading;
using Microsoft.IdentityModel.Claims;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider
{
	public class WindowsAccountProvider : IWindowsAccountProvider
	{
		private readonly ICurrentHttpContext _httpContext;
		
		public WindowsAccountProvider(ICurrentHttpContext httpContext)
		{
			_httpContext = httpContext;
		}

		public WindowsAccount RetrieveWindowsAccount()
		{
			var currentIdentity = Thread.CurrentPrincipal.Identity as IClaimsIdentity;
			if (currentIdentity == null)
			{
				return null;
			}

			// err-hantering + test för detta senare... + browserstöd.
			var splitBySeparator = _httpContext.Current().Request.ServerVariables["LOGON_USER"].Split('\\');
			if(splitBySeparator.Length != 2)
			{
				return null;
			}

			var domainName = splitBySeparator[0];
			var userName = splitBySeparator[1];
			return new WindowsAccount(domainName, userName);
		}
	}
}