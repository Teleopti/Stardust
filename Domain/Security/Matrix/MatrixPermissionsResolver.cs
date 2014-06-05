using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.Matrix
{
    public class MatrixPermissionsResolver
    {
        private readonly IList<IPersonRoleResolver> _personRoleResolvers;

        protected MatrixPermissionsResolver(IList<IPersonRoleResolver> personRoleResolvers)
        {
            _personRoleResolvers = personRoleResolvers;
        }

        public MatrixPermissionsResolver(IPersonRepository personRepository, IFunctionsForRoleProvider functionsForRoleProvider, ISiteRepository siteRepository)
        {
            _personRoleResolvers = new List<IPersonRoleResolver>();
            foreach (IPerson person in personRepository.FindAllSortByName(true))
            {
                ITeamResolver teamResolver = new TeamResolver(person,siteRepository);
                IApplicationFunctionResolver functionResolver = new ApplicationFunctionResolver(functionsForRoleProvider);
                _personRoleResolvers.Add(new PersonRoleResolver(person, teamResolver, functionResolver));
            }
        }

        public IList<MatrixPermissionHolder> ResolvePermission(DateOnly queryDate, IUnitOfWorkFactory unitOfWorkFactory)
        {
            using (PerformanceOutput.ForOperation("MatrixPermissionWithoutLoading"))
            {
                IList<MatrixPermissionHolder> result = new List<MatrixPermissionHolder>();
                foreach (IPersonRoleResolver resolver in _personRoleResolvers)
                {

                    HashSet<MatrixPermissionHolder> resolverResult = resolver.Resolve(queryDate,unitOfWorkFactory);
                    foreach (MatrixPermissionHolder holder in resolverResult)
                    {
                        result.Add(holder);
                    }
                }
                return result;
            }
        }
    }
}
