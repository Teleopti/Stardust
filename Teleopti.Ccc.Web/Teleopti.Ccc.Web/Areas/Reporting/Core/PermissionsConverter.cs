using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Reporting.Core
{
	public class PermissionsConverter : IPermissionsConverter
	{
		private readonly IAnalyticsTeamRepository _analyticsTeamRepository;
		private readonly INow _now;
		private readonly IApplicationPermissionProvider _applicationPermissionProvider;

		public PermissionsConverter(IAnalyticsTeamRepository analyticsTeamRepository, INow now, IApplicationPermissionProvider applicationPermissionProvider)
		{
			_analyticsTeamRepository = analyticsTeamRepository;
			_now = now;
			_applicationPermissionProvider = applicationPermissionProvider;
		}

		public ICollection<AnalyticsPermission> GetApplicationPermissionsAndConvert(Guid personId, int analyticsBusinessUnitId)
		{
			var applicationPermissions = _applicationPermissionProvider.GetPermissions(personId);
			var analyticTeams = _analyticsTeamRepository.GetTeams();
			var now = _now.UtcDateTime();

			return
				applicationPermissions.Select(ap => convertToAnalyticsPermission(ap, analyticTeams, analyticsBusinessUnitId, now))
					.Where(x => x != null)
					.ToList();
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