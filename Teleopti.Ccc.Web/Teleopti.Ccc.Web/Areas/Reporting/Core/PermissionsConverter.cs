using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Web.Areas.Reporting.Core
{
	public class PermissionsConverter : IPermissionsConverter
	{
		private readonly IAnalyticsTeamRepository _analyticsTeamRepository;
		private readonly INow _now;
		private readonly IPersonRepository _personRepository;
		private readonly ISiteRepository _siteRepository;
		private readonly ISpecification<IApplicationFunction> _matrixFunctionSpecification =
			 new ExternalApplicationFunctionSpecification(DefinedForeignSourceNames.SourceMatrix);

		public PermissionsConverter(IAnalyticsTeamRepository analyticsTeamRepository, INow now, IPersonRepository personRepository, ISiteRepository siteRepository)
		{
			_analyticsTeamRepository = analyticsTeamRepository;
			_now = now;
			_personRepository = personRepository;
			_siteRepository = siteRepository;
		}

		public IEnumerable<AnalyticsPermission> GetApplicationPermissionsAndConvert(Guid personId, int analyticsBusinessUnitId)
		{
			var analyticTeams = _analyticsTeamRepository.GetTeams();
			var now = _now.UtcDateTime();
			var person = _personRepository.Get(personId);
			var sites = _siteRepository.LoadAll();
			var teamResolver = new TeamResolver(person, sites);
			var result = new List<AnalyticsPermission>();
			foreach (var role in person.PermissionInformation.ApplicationRoleCollection)
			{
				var teams = teamResolver.ResolveTeams(role, new DateOnly(now));
				foreach (var function in role.ApplicationFunctionCollection.FilterBySpecification(_matrixFunctionSpecification))
				{
					foreach (var stuff in teams)
					{
						var analyticsPermission = convertToAnalyticsPermission(new MatrixPermissionHolder(person, stuff.Team, stuff.IsMy, function), analyticTeams, analyticsBusinessUnitId, now);
						if (analyticsPermission != null)
							result.Add(analyticsPermission);
					}
				}
			}
			return result.Distinct();
		}

		private static AnalyticsPermission convertToAnalyticsPermission(MatrixPermissionHolder arg, IEnumerable<AnalyticTeam> analyticTeams, int analyticsBusinessUnitId, DateTime updateDate)
		{
			var analyticsTeam = analyticTeams.FirstOrDefault(t => t.TeamCode == arg.Team.Id);
			if (analyticsTeam == null)
			{
				return null;
			}
			return new AnalyticsPermission
			{
				PersonCode = arg.Person.Id.GetValueOrDefault(),
				DatasourceId = 1,
				ReportId = new Guid(arg.ApplicationFunction.ForeignId),
				BusinessUnitId = analyticsBusinessUnitId,
				MyOwn = arg.IsMy,
				TeamId = analyticsTeam.TeamId,
				DatasourceUpdateDate = updateDate
			};
		}
	}
}