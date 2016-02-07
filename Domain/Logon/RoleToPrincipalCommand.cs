using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Logon
{
    public class RoleToPrincipalCommand : IRoleToPrincipalCommand
    {
        private readonly ClaimSetForApplicationRole _claimSetForApplicationRole;

        public RoleToPrincipalCommand(ClaimSetForApplicationRole claimSetForApplicationRole)
        {
            _claimSetForApplicationRole = claimSetForApplicationRole;
        }

		public void Execute(ITeleoptiPrincipal principalToFillWithClaimSets, IUnitOfWorkFactory unitOfWorkFactory, IPersonRepository personRepository)
        {
            var person = principalToFillWithClaimSets.GetPerson(personRepository);
            foreach (var applicationRole in person.PermissionInformation.ApplicationRoleCollection)
            {
	            principalToFillWithClaimSets.AddClaimSet(_claimSetForApplicationRole.Transform(applicationRole, unitOfWorkFactory.Name));
            }
        }
    }
}