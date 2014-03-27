using System;
using System.Linq;
using Microsoft.IdentityModel.Claims;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider
{
	public class TokenIdentityProvider : ITokenIdentityProvider
	{
		private readonly ICurrentHttpContext _httpContext;
		
		public TokenIdentityProvider(ICurrentHttpContext httpContext)
		{
			_httpContext = httpContext;
		}

		public TokenIdentity RetrieveToken()
		{
			if (_httpContext.Current().User == null)
				return null;
			var teleoptiIdentity = _httpContext.Current().User.Identity as ITeleoptiIdentity;
			if (teleoptiIdentity != null)
			{
				return getTokenIdentity(teleoptiIdentity.TokenIdentity);
			}

			var currentIdentity = _httpContext.Current().User.Identity as IClaimsIdentity;
			if (currentIdentity == null)
			{
				return null;
			}

			var nameClaim = currentIdentity.Claims.FirstOrDefault(x => x.ClaimType == ClaimTypes.NameIdentifier);
			var nameClaimValue = Uri.UnescapeDataString(nameClaim.Value);
			return getTokenIdentity(nameClaimValue);
		}

		private static TokenIdentity getTokenIdentity(string nameClaimValue)
		{
			return nameClaimValue.Contains("#")
				? getWindowsTokenIdentity(nameClaimValue)
				: getApplicationTokenIdentity(nameClaimValue);
		}

		private static TokenIdentity getApplicationTokenIdentity(string nameClaimValue)
		{
			var nameAndDatasource = nameClaimValue.Split('/').Last().Split('§');
			return new TokenIdentity
			{
				DataSource = nameAndDatasource[1],
				UserIdentifier = nameAndDatasource[0],
				OriginalToken = nameClaimValue
			};
		}

		private static TokenIdentity getWindowsTokenIdentity(string nameClaimValue)
		{
			var nameAndDomain = nameClaimValue.Split('/').Last().Split('#');
			return new TokenIdentity
			{
				UserDomain = nameAndDomain[1],
				UserIdentifier = nameAndDomain[0],
				OriginalToken = nameClaimValue
			};
		}
	}

	public class TokenIdentity
	{
		public string UserIdentifier { get; set; }
		public string UserDomain { get; set; }
		public string OriginalToken { get; set; }
		public string DataSource { get; set; }
	}
}