﻿using System.Collections.Generic;
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

        public MatrixPermissionsResolver(IPersonProvider provider, IFunctionsForRoleProvider functionsForRoleProvider, ISiteRepository siteRepository)
        {
            _personRoleResolvers = new List<IPersonRoleResolver>();
            foreach (IPerson person in provider.GetPersons())
            {
                ITeamResolver teamResolver = new TeamResolver(person,siteRepository);
                IApplicationFunctionResolver functionResolver = new ApplicationFunctionResolver(functionsForRoleProvider);
                _personRoleResolvers.Add(new PersonRoleResolver(person, teamResolver, functionResolver));
            }
        }

        public IList<MatrixPermissionHolder> ResolvePermission(DateOnly queryDate, IUnitOfWork unitOfWork)
        {
            using (PerformanceOutput.ForOperation("MatrixPermissionWithoutLoading"))
            {
                IList<MatrixPermissionHolder> result = new List<MatrixPermissionHolder>();
                foreach (IPersonRoleResolver resolver in _personRoleResolvers)
                {

                    HashSet<MatrixPermissionHolder> resolverResult = resolver.Resolve(queryDate,unitOfWork);
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
