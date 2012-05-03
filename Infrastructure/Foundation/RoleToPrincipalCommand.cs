using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
    public class RoleToPrincipalCommand : IRoleToPrincipalCommand
    {
        private readonly IRoleToClaimSetTransformer _roleToClaimSetTransformer;

        public RoleToPrincipalCommand(IRoleToClaimSetTransformer roleToClaimSetTransformer)
        {
            _roleToClaimSetTransformer = roleToClaimSetTransformer;
        }

        public void Execute(ITeleoptiPrincipal principalToFillWithClaimSets, IUnitOfWork unitOfWork, IPersonRepository personRepository)
        {
            var person = principalToFillWithClaimSets.GetPerson(personRepository);
            foreach (var applicationRole in person.PermissionInformation.ApplicationRoleCollection)
            {
                principalToFillWithClaimSets.AddClaimSet(_roleToClaimSetTransformer.Transform(applicationRole,unitOfWork));
            }
        }
    }
}