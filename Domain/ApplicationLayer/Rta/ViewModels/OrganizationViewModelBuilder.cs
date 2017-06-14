using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class OrganizationViewModelBuilder
	{
		private readonly ICurrentAuthorization _authorization;
		private readonly IOrganizationReader _organizationReader;
		private readonly IUserNow _userNow;

		public OrganizationViewModelBuilder(
			ICurrentAuthorization authorization,
			IOrganizationReader organizationReader, 
			IUserNow userNow)
		{
			_authorization = authorization;
			_organizationReader = organizationReader;
			_userNow = userNow;
		}

		public IEnumerable<OrganizationSiteViewModel> Build()
		{
			return BuildForSkills(null);
		}

		public IEnumerable<OrganizationSiteViewModel> BuildForSkills(Guid[] skillIds)
		{
			var sites = skillIds == null ?
					_organizationReader.Read() :
					_organizationReader.Read(skillIds)
				;


			var auth = _authorization.Current();
			var rtaOverview = DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview;

			return (
				from s in sites
				let teams = (from t in s.Teams
					where auth.IsPermitted(rtaOverview, _userNow.Date(),
						new TeamAuthorization {BusinessUnitId = s.BusinessUnitId, SiteId = s.SiteId, TeamId = t.TeamId})
					select t).ToArray()
				where auth.IsPermitted(rtaOverview, _userNow.Date(),
						  new SiteAuthorization {BusinessUnitId = s.BusinessUnitId, SiteId = s.SiteId}) ||
					  teams.Any()
				select new OrganizationSiteViewModel
				{
					Id = s.SiteId,
					Name = s.SiteName,
					Teams = from t in teams
						select new OrganizationTeamViewModel
						{
							Id = t.TeamId,
							Name = t.TeamName
						}
				}
			).ToArray();
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