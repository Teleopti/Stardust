using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Logon
{
	public class RoleToPrincipalCommand : IRoleToPrincipalCommand
	{
		private readonly ClaimSetForApplicationRole _claimSetForApplicationRole;

		public RoleToPrincipalCommand(ClaimSetForApplicationRole claimSetForApplicationRole)
		{
			_claimSetForApplicationRole = claimSetForApplicationRole;
		}

		public void Execute(ITeleoptiPrincipal principal, IPersonRepository personRepository, string tenantName) =>
			Execute(new PrincipalPersonOwner(principal), principal, personRepository, tenantName);

		public void Execute(IPersonOwner personOwner, IClaimsOwner claimsOwner, IPersonRepository personRepository, string tenantName)
		{
			var person = personOwner.GetPerson(personRepository);
			execute(person, claimsOwner, tenantName);
		}

		private void execute(IPerson person, IClaimsOwner claimsOwner, string tenantName)
		{
			foreach (var applicationRole in person.PermissionInformation.ApplicationRoleCollection)
			{
				claimsOwner.AddClaimSet(_claimSetForApplicationRole.Transform(applicationRole, tenantName));
			}
		}
	}
}