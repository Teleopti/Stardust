using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Teleopti.Ccc.Web.Filters
{
	public class KeyBasedAuthenticationAttribute : BasicAuthenticationAttribute
	{
		protected override async Task<IPrincipal> AuthenticateAsync(string userName, string password, CancellationToken cancellationToken)
		{

			var identity = new ClaimsIdentity();
			return new ClaimsPrincipal(identity);
		}

	}


}