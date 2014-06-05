using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.Matrix
{
    public interface IPersonRoleResolver
    {
        HashSet<MatrixPermissionHolder> Resolve(DateOnly queryDate, IUnitOfWorkFactory unitOfWorkFactory);
    }

    public class PersonRoleResolver : IPersonRoleResolver
    {
        private readonly IPerson _person;
        private readonly ITeamResolver _teamResolverRole;
        private readonly IApplicationFunctionResolver _applicationFunctionResolver;

        public PersonRoleResolver(
            IPerson person,
            ITeamResolver teamResolverRole,
            IApplicationFunctionResolver applicationFunctionResolver)
        {
            _person = person;
            _teamResolverRole = teamResolverRole;
            _applicationFunctionResolver = applicationFunctionResolver;
        }

        public HashSet<MatrixPermissionHolder> Resolve(DateOnly queryDate, IUnitOfWorkFactory unitOfWorkFactory)
        {
            HashSet<MatrixPermissionHolder> result = new HashSet<MatrixPermissionHolder>();

            foreach (IApplicationRole role in _person.PermissionInformation.ApplicationRoleCollection)
            {
                HashSet<MatrixPermissionHolder> roleResult =
                    _teamResolverRole.ResolveTeams(role, queryDate);
                HashSet<MatrixPermissionHolder> appFunctionResult =
                    _applicationFunctionResolver.ResolveApplicationFunction(roleResult, role, unitOfWorkFactory);
                foreach (MatrixPermissionHolder teamHolder in appFunctionResult)
                {
                    result.Add(teamHolder);
                }
            }
            return result;
        }
    }
}