using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Security.Matrix
{
	public class MatrixPermissionsResolver
	{
		private readonly IList<IPersonRoleResolver> _personRoleResolvers;

		protected MatrixPermissionsResolver(IList<IPersonRoleResolver> personRoleResolvers)
		{
			_personRoleResolvers = personRoleResolvers;
		}

		public MatrixPermissionsResolver(IPersonRepository personRepository, ApplicationFunctionsForRole applicationFunctionsForRole, ISiteRepository siteRepository)
		{
			_personRoleResolvers = new List<IPersonRoleResolver>();
			var sites = siteRepository.LoadAll();
			foreach (IPerson person in personRepository.FindAllWithRolesSortByName())
			{
				ITeamResolver teamResolver = new TeamResolver(person, sites);
				IApplicationFunctionResolver functionResolver = new ApplicationFunctionResolver(applicationFunctionsForRole);
				_personRoleResolvers.Add(new PersonRoleResolver(person, teamResolver, functionResolver));
			}
		}

		public IList<MatrixPermissionHolder> ResolvePermission(DateOnly queryDate, IUnitOfWorkFactory unitOfWorkFactory)
		{
			using (PerformanceOutput.ForOperation("MatrixPermissionWithoutLoading"))
			{
				var result = new List<MatrixPermissionHolder>();
				foreach (IPersonRoleResolver resolver in _personRoleResolvers)
				{
					result.AddRange(resolver.Resolve(queryDate, unitOfWorkFactory));
				}
				return result;
			}
		}
	}
}
