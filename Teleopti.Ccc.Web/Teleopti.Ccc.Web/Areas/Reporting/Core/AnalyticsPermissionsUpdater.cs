using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Reporting.Core
{
	public class AnalyticsPermissionsUpdater : IAnalyticsPermissionsUpdater
	{
		private readonly IPersonRepository _personRepository;
		private readonly ISiteRepository _siteRepository;
		private readonly IApplicationFunctionRepository _applicationFunctionRepository;
		private readonly INow _now;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly IAnalyticsPermissionRepository _analyticsPermissionRepository;
		private readonly IAnalyticsTeamRepository _analyticsTeamRepository;
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private readonly IAnalyticsPermissionExecutionRepository _analyticsPermissionExecutionRepository;

		public AnalyticsPermissionsUpdater(IPersonRepository personRepository,
			ISiteRepository siteRepository,
			IApplicationFunctionRepository applicationFunctionRepository,
			INow now,
			ICurrentDataSource currentDataSource,
			IAnalyticsPermissionRepository analyticsPermissionRepository,
			IAnalyticsTeamRepository analyticsTeamRepository,
			IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository,
			IAnalyticsPermissionExecutionRepository analyticsPermissionExecutionRepository)
		{
			_personRepository = personRepository;
			_siteRepository = siteRepository;
			_applicationFunctionRepository = applicationFunctionRepository;
			_now = now;
			_currentDataSource = currentDataSource;
			_analyticsPermissionRepository = analyticsPermissionRepository;
			_analyticsTeamRepository = analyticsTeamRepository;
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
			_analyticsPermissionExecutionRepository = analyticsPermissionExecutionRepository;
		}

		public void Handle(Guid personId, Guid businessUnitId)
		{
			var lastUpdate = _analyticsPermissionExecutionRepository.Get(personId);
			if (DateTime.UtcNow - lastUpdate < TimeSpan.FromMinutes(15))
				return;

			var sites = _siteRepository.LoadAll();
			var analyticTeams = _analyticsTeamRepository.GetTeams();
			var businessUnit = _analyticsBusinessUnitRepository.Get(businessUnitId);
			var currentPermissions = getPermissionsFromAppDatabase(personId, sites, analyticTeams, businessUnit);
			var currentAnalyticsPermissions = _analyticsPermissionRepository.GetPermissionsForPerson(personId);

			var toBeAdded = currentPermissions.Where(p => !currentAnalyticsPermissions.Any(x => x.Equals(p)));
			var toBeDeleted = currentAnalyticsPermissions.Where(p => !currentPermissions.Any(x => x.Equals(p)));
			_analyticsPermissionRepository.InsertPermissions(toBeAdded);
			_analyticsPermissionRepository.DeletePermissions(toBeDeleted);

			_analyticsPermissionExecutionRepository.Set(personId);
		}

		private IList<AnalyticsPermission> getPermissionsFromAppDatabase(Guid personId, IList<ISite> sites, IList<AnalyticTeam> analyticTeams, AnalyticBusinessUnit businessUnit)
		{
			var person = _personRepository.Get(personId);
			ITeamResolver teamResolver = new TeamResolver(person, sites);
			var functionResolver = new ApplicationFunctionResolver(new ApplicationFunctionsForRole(new LicensedFunctionsProvider(new DefinedRaptorApplicationFunctionFactory()), _applicationFunctionRepository));
			var personRoleResolver = new PersonRoleResolver(person, teamResolver, functionResolver);
			var now = _now.UtcDateTime();
			var currentPermissions = personRoleResolver.Resolve(new DateOnly(now), _currentDataSource.Current().Application)
				.Select(p => convertToAnalyticsPermission(p, analyticTeams, businessUnit, now)).ToList();
			return currentPermissions;
		}

		private static AnalyticsPermission convertToAnalyticsPermission(MatrixPermissionHolder arg, IEnumerable<AnalyticTeam> analyticTeams, AnalyticBusinessUnit businessUnit, DateTime updateDate)
		{
			var analyticsTeam = analyticTeams.First(t => t.TeamCode == arg.Team.Id); // TODO: What about a new team that is not in analytics yet?
			return new AnalyticsPermission
			{
				PersonCode = arg.Person.Id.GetValueOrDefault(),
				DatasourceId = 1,
				ReportId = new Guid(arg.ApplicationFunction.ForeignId),
				BusinessUnitId = businessUnit.BusinessUnitId,
				MyOwn = arg.IsMy,
				TeamId = analyticsTeam.TeamId,
				DatasourceUpdateDate = updateDate
			};
		}
	}

	public interface IAnalyticsPermissionsUpdater
	{
		void Handle(Guid personId, Guid businessUnitId);
	}
}