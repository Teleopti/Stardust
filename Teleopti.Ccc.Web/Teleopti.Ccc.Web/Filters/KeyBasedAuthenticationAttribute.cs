using System.Collections.Generic;
using System.Configuration;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
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
				new Claim(System.IdentityModel.Claims.ClaimTypes.NameIdentifier, userName ),
				// would be possible to send the datasource (the name in the nhib) as a claim
				new Claim(System.IdentityModel.Claims.ClaimTypes.Locality, userName )
			};
			var claimsIdentity = new ClaimsIdentity(claims, "Basic", System.IdentityModel.Claims.ClaimTypes.NameIdentifier,"");
			
			return await Task.FromResult(new ClaimsPrincipal(claimsIdentity));
		}
	}
}