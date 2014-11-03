using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Claims;
using Teleopti.Ccc.Infrastructure.Foundation;
using Claim = System.Security.Claims.Claim;
using ClaimsIdentity = System.Security.Claims.ClaimsIdentity;
using ClaimsPrincipal = System.Security.Claims.ClaimsPrincipal;

namespace Teleopti.Ccc.Web.Filters
{
	public class KeyBasedAuthenticationAttribute : BasicAuthenticationAttribute
	{
		protected override async Task<IPrincipal> AuthenticateAsync(string userName, string password, CancellationToken cancellationToken)
		{
			var key = ConfigurationManager.AppSettings["AuthenticationKey"];
			if (!password.Equals(key))
				return null;
			var claims = new List<Claim>
			{
				new Claim(System.IdentityModel.Claims.ClaimTypes.NameIdentifier, userName )
			};
			var claimsIdentity = new ClaimsIdentity(claims, "Basic");
			
			
			return new ClaimsPrincipal(claimsIdentity);
		}

	}


}