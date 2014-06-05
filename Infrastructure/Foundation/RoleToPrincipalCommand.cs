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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Execute(ITeleoptiPrincipal principalToFillWithClaimSets, IUnitOfWorkFactory unitOfWorkFactory, IPersonRepository personRepository)
        {
            var person = principalToFillWithClaimSets.GetPerson(personRepository);
            foreach (var applicationRole in person.PermissionInformation.ApplicationRoleCollection)
            {
                principalToFillWithClaimSets.AddClaimSet(_roleToClaimSetTransformer.Transform(applicationRole,unitOfWorkFactory));
            }
        }
    }
}