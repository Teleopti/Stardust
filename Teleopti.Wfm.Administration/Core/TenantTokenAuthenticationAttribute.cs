using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Administration.Core
{
	public class TenantTokenAuthenticationAttribute : Attribute, IAuthenticationFilter
	{
		public virtual bool AllowMultiple => false;

		public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
		{
			var cookieAuth = "";
			if (HttpContext.Current.Request.Cookies.AllKeys.Contains("WfmAdminAuth"))
			{
				var cook = HttpContext.Current.Request.Cookies["WfmAdminAuth"];
				var value = Uri.UnescapeDataString(cook.Value);
				var obj = Json.Decode<cookieValues>(value);
				cookieAuth = obj.tokenKey;
			}
			var req = context.Request;
			if (req.Headers.Authorization != null &&
					req.Headers.Authorization.Scheme.Equals(
							  "Bearer", StringComparison.OrdinalIgnoreCase) || cookieAuth != "")
			{
				var auth = cookieAuth != "" ? cookieAuth : req.Headers.Authorization.Parameter;
				//Tenant: todo look up so key exists in user db
				var valid = AdminAccessTokenRepository.TokenIsValid(auth, new Now());
			
				if (valid)
				{
					var claims = new List<Claim>
				{
					new Claim(ClaimTypes.Name, "Admin")
				};
					var id = new ClaimsIdentity(claims, "Basic");
					var principal = new ClaimsPrincipal(new[] { id });
					context.Principal = principal;
				}
				else
				{
					context.ErrorResult = new UnauthorizedResult(
						 new AuthenticationHeaderValue[0],
											  context.Request);
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

	class cookieValues
	{
	public string tokenKey { get; set; }
	public string user { get; set; }
		public int id { get; set; }
	}
}