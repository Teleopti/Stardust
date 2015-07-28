using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Http.Results;

namespace Teleopti.Wfm.Administration.Core
{
	public class TenantTokenAuthenticationAttribute : Attribute, IAuthenticationFilter
	{
		public virtual bool AllowMultiple
		{
			get
			{
				return false;
			} 
		}

		public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
		{
			var req = context.Request;
			if (req.Headers.Authorization != null &&
					req.Headers.Authorization.Scheme.Equals(
							  "Bearer", StringComparison.OrdinalIgnoreCase))
			{
				string auth = req.Headers.Authorization.Parameter;
				//Tenant: todo look up so key exists in user db
				
				if (!auth.Equals("")) // Just a dumb check
				{
					var claims = new List<Claim>()
				{
					new Claim(ClaimTypes.Name, "Admin")
				};
					var id = new ClaimsIdentity(claims, "Basic");
					var principal = new ClaimsPrincipal(new[] { id });
					context.Principal = principal;
				}
			}
			else
			{
				context.ErrorResult = new UnauthorizedResult(
						 new AuthenticationHeaderValue[0],
											  context.Request);
			}

			return Task.FromResult(0);
		}

		public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
		{
			return Task.FromResult(0);
		}
	}
}