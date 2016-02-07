using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Logon
{
	public class ClaimSetForApplicationRole
    {
        private readonly ApplicationFunctionsForRole _functionsForRoleProvider;

		public ClaimSetForApplicationRole(ApplicationFunctionsForRole functionsForRoleProvider)
        {
        	_functionsForRoleProvider = functionsForRoleProvider;
        }

		public ClaimSet Transform(IApplicationRole role, string tenantName)
        {
            var claims = new List<Claim>();

            var availableFunctions = _functionsForRoleProvider.AvailableFunctions(role, tenantName);

			claims.AddRange(
				availableFunctions
					.Select(applicationFunction =>
					        new Claim(
					        	string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace, "/", applicationFunction.FunctionPath),
								applicationFunction,
					        	Rights.PossessProperty
					        	))
				);

			if (role.AvailableData != null)
			{
				claims.Add(new Claim(string.Concat(
					TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace, "/AvailableData"),
				                     new AuthorizeExternalAvailableData(role.AvailableData), Rights.PossessProperty));

				IAuthorizeAvailableData authorizeAvailableData = null;
				switch (role.AvailableData.AvailableDataRange)
				{
					case AvailableDataRangeOption.Everyone:
						authorizeAvailableData = new AuthorizeEveryone();
						break;
					case AvailableDataRangeOption.MyBusinessUnit:
						authorizeAvailableData = new AuthorizeMyBusinessUnit();
						break;
					case AvailableDataRangeOption.MyOwn:
						authorizeAvailableData = new AuthorizeMyOwn();
						break;
					case AvailableDataRangeOption.MySite:
						authorizeAvailableData = new AuthorizeMySite();
						break;
					case AvailableDataRangeOption.MyTeam:
						authorizeAvailableData = new AuthorizeMyTeam();
						break;
				}
				if (authorizeAvailableData != null)
				{
					claims.Add(
						new Claim(
							string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace,
							              "/AvailableData"), authorizeAvailableData, Rights.PossessProperty));
				}
			}

        	return new DefaultClaimSet(ClaimSet.System,claims);
        }
    }
}