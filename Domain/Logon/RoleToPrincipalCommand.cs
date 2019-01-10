using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
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

		public void Execute(IPersonOwner principalToFillWithClaimSets, IClaimsOwner claimsOwner, IUnitOfWorkFactory unitOfWorkFactory, IPersonRepository personRepository)
        {
            var person = principalToFillWithClaimSets.GetPerson(personRepository);
            foreach (var applicationRole in person.PermissionInformation.ApplicationRoleCollection)
            {
	            claimsOwner.AddClaimSet(_claimSetForApplicationRole.Transform(applicationRole, unitOfWorkFactory.Name));
            }
        }
    }
}