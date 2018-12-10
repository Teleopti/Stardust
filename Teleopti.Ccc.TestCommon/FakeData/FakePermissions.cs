using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakePermissions: IAuthorization, ICurrentAuthorization
	{
		private readonly IList<string> _permittedAnyData = new List<string>();
		private readonly IList<Tuple<string, Guid>> _permittedSites = new List<Tuple<string, Guid>>();
		private readonly IList<Tuple<string, Guid>> _permittedTeams = new List<Tuple<string, Guid>>();

		public bool IsPermitted(string functionPath, DateOnly dateOnly, IPerson person)
		{
			return _permittedAnyData.Contains(functionPath);
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, ITeam team)
		{
			return IsPermitted(functionPath, dateOnly, new TeamAuthorization
			{
				BusinessUnitId = (team.Site?.BusinessUnit?.Id).GetValueOrDefault(),
				SiteId = (team.Site?.Id).GetValueOrDefault(),
				TeamId = team.Id.Value
			});
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, ISite site)
		{
			return IsPermitted(functionPath, dateOnly, new SiteAuthorization
			{
				BusinessUnitId = (site.BusinessUnit?.Id).GetValueOrDefault(),
				SiteId = site.Id.Value
			});
		}

		public bool IsPermitted(string functionPath)
		{
			return _permittedAnyData.Contains(functionPath);
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, IPersonAuthorization authorization)
		{
			return _permittedAnyData.Contains(functionPath);
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, ITeamAuthorization authorization)
		{
			return _permittedAnyData.Contains(functionPath) ||
				   _permittedTeams.Any(x => x.Item1 == functionPath && x.Item2 == authorization.TeamId);
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, ISiteAuthorization authorization)
		{
			return _permittedAnyData.Contains(functionPath) ||
				   _permittedSites.Any(x => x.Item1 == functionPath && x.Item2 == authorization.SiteId);
		}

		public IEnumerable<DateOnlyPeriod> PermittedPeriods(string functionPath, DateOnlyPeriod period, IPerson person)
		{
			if (_permittedAnyData.Contains(functionPath))
				return new List<DateOnlyPeriod> { period };
			return new List<DateOnlyPeriod>();
		}

		public IEnumerable<IApplicationFunction> GrantedFunctions()
		{
			var grantedFunctions = new List<IApplicationFunction>();
			_permittedAnyData.ForEach(p => grantedFunctions.Add(new ApplicationFunction(p)));
			return grantedFunctions;
		}

		public bool EvaluateSpecification(ISpecification<IEnumerable<ClaimSet>> specification)
		{
			throw new NotImplementedException();
		}

		public void HasPermission(string functionPath)
		{
			_permittedAnyData.Add(functionPath);
		}

		public void HasPermissionToSite(string functionPath, Guid siteId)
		{
			_permittedSites.Add(new Tuple<string, Guid>(functionPath, siteId));
		}

		public void HasPermissionToTeam(string functionPath, Guid teamId)
		{
			_permittedTeams.Add(new Tuple<string, Guid>(functionPath, teamId));
		}

		public IAuthorization Current()
		{
			return this;
		}
	}
}