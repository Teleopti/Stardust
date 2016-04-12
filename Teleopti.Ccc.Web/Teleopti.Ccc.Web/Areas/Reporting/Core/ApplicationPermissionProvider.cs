using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Reporting.Core
{
	public class ApplicationPermissionProvider : IApplicationPermissionProvider
	{
		private readonly IPersonRepository _personRepository;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly ISiteRepository _siteRepository;
		private readonly INow _now;
		private readonly IApplicationFunctionResolver _applicationFunctionResolver;

		public ApplicationPermissionProvider(IPersonRepository personRepository, ICurrentDataSource currentDataSource, ISiteRepository siteRepository, INow now, IApplicationFunctionResolver applicationFunctionResolver)
		{
			_personRepository = personRepository;
			_currentDataSource = currentDataSource;
			_siteRepository = siteRepository;
			_now = now;
			_applicationFunctionResolver = applicationFunctionResolver;
		}

		public ICollection<MatrixPermissionHolder> GetPermissions(Guid personId)
		{
			var person = _personRepository.Get(personId);
			var sites = _siteRepository.LoadAll();
			ITeamResolver teamResolver = new TeamResolver(person, sites);

			var personRoleResolver = new PersonRoleResolver(person, teamResolver, _applicationFunctionResolver);
			var now = _now.UtcDateTime();
			var currentPermissions = personRoleResolver.Resolve(new DateOnly(now), _currentDataSource.Current().Application);
			return currentPermissions;
		}
	}
}