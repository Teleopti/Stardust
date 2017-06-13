using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class OrganizationViewModelBuilder
	{
		private readonly INow _now;
		private readonly ICurrentAuthorization _authorization;
		private readonly ITeamCardReader _teamCardReader;

		public OrganizationViewModelBuilder(
			INow now,
			ICurrentAuthorization authorization,
			ITeamCardReader teamCardReader)
		{
			_now = now;
			_authorization = authorization;
			_teamCardReader = teamCardReader;
		}

		public IEnumerable<OrganizationSiteViewModel> Build()
		{

			return BuildForSkills(null);
			
		}

		public IEnumerable<OrganizationSiteViewModel> BuildForSkills(Guid[] skillIds)
		{
			var teams = skillIds == null ?
					_teamCardReader.Read() :
					_teamCardReader.Read(skillIds)
				;

			var auth = _authorization.Current();

			var rtaOverview = DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview;
			return
				teams
					.Where(x =>
						auth.IsPermitted(rtaOverview, _now.ServerDate_DontUse(),
							new SiteAuthorization {BusinessUnitId = x.BusinessUnitId, SiteId = x.SiteId}) ||
						auth.IsPermitted(rtaOverview, _now.ServerDate_DontUse(),
							new TeamAuthorization {BusinessUnitId = x.BusinessUnitId, SiteId = x.SiteId, TeamId = x.TeamId})
					)
					.GroupBy(x => x.SiteId)
					.Select(site =>
						new OrganizationSiteViewModel
						{
							Id = site.Key,
							Name = site.FirstOrDefault()?.SiteName,
							Teams = site
								.Where(t =>
									auth.IsPermitted(rtaOverview, _now.ServerDate_DontUse(),
										new TeamAuthorization {BusinessUnitId = t.BusinessUnitId, SiteId = t.SiteId, TeamId = t.TeamId}))
								.Select(
									team => new OrganizationTeamViewModel
									{
										Id = team.TeamId,
										Name = team.TeamName
									}).ToArray()

						})
						.ToArray();
		}
	}

	public class OrganizationSiteViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public IEnumerable<OrganizationTeamViewModel> Teams;
	}

	public class OrganizationTeamViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
	}
}