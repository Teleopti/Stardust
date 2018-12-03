using System.IdentityModel.Claims;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;

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
            var availableFunctions = _functionsForRoleProvider.AvailableFunctions(role, tenantName);

			var claims = availableFunctions
					.Select(applicationFunction =>
					        new Claim(
					        	string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace, "/", applicationFunction.FunctionPath),
								applicationFunction,
					        	Rights.PossessProperty
					        	)).ToList();

			if (role.AvailableData != null)
			{
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
				claims.Add(new Claim(string.Concat(
						TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace, "/AvailableData"),
					AuthorizeExternalAvailableData.Create(role.AvailableData), Rights.PossessProperty));
			}

        	return new DefaultClaimSet(ClaimSet.System,claims);
        }
    }
}