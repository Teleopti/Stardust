using System;
using System.Linq;
using System.Security.Claims;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class TokenIdentityProvider : ITokenIdentityProvider
	{
		public const char ApplicationIdentifier = '@';

		private readonly ICurrentHttpContext _httpContext;
		
		public TokenIdentityProvider(ICurrentHttpContext httpContext)
		{
			_httpContext = httpContext;
		}

		public TokenIdentity RetrieveToken()
		{
			var httpContext = _httpContext.Current();
			if (httpContext == null) return null;
			if (httpContext.User == null) return null;

			var teleoptiIdentity = httpContext.User.Identity as ITeleoptiIdentity;
			if (teleoptiIdentity != null)
			{
				return teleoptiIdentity.TokenIdentity != null ? getTokenIdentity(teleoptiIdentity.TokenIdentity) : null;
			}

			var currentIdentity = httpContext.User.Identity as ClaimsIdentity;
			if (currentIdentity == null)
			{
				return null;
			}

			var nameClaim = currentIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
			var isPersistentClaim = currentIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.IsPersistent);
			if (nameClaim != null)
			{
				var nameClaimValue = Uri.UnescapeDataString(nameClaim.Value);
				var token = getTokenIdentity(nameClaimValue);
				token.IsPersistent = isPersistentClaim != null && isPersistentClaim.Value.ToLowerInvariant() == "true";

				return token;
			}
			return null;
		}

		private static TokenIdentity getTokenIdentity(string nameClaimValue)
		{
			return nameClaimValue.EndsWith(ApplicationIdentifier.ToString())
				? getApplicationTokenIdentity(nameClaimValue)
				: getWindowsTokenIdentity(nameClaimValue);
		}

		private static TokenIdentity getApplicationTokenIdentity(string nameClaimValue)
		{
			var nameAndDatasource = nameClaimValue.Split('/').Last().TrimEnd(ApplicationIdentifier);
			return new TokenIdentity
			{
				UserIdentifier = nameAndDatasource,
				OriginalToken = nameClaimValue,
				IsTeleoptiApplicationLogon = true
			};
		}

		private static TokenIdentity getWindowsTokenIdentity(string nameClaimValue)
		{
			var nameAndDomain = nameClaimValue.Split('/').Last();
			return new TokenIdentity
			{
				UserIdentifier = nameAndDomain.Replace("#","\\").Replace("$$$" , "."),
				OriginalToken = nameClaimValue,
				IsTeleoptiApplicationLogon = false
			};
		}
	}
}