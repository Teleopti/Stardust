using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class FakePermissionProvider : IPermissionProvider
	{
		private readonly Dictionary<string, DateOnly?> _applicationFunctions = new Dictionary<string, DateOnly?>();
		private readonly IList<PersonPermissionData> _personPermissionDatas = new List<PersonPermissionData>();
		private readonly IList<SitePermissionData> _sitePermissionData = new List<SitePermissionData>();
		private readonly IList<TeamPermissionData> _teamPermissionData = new List<TeamPermissionData>();
		private DateOnly? _schedulePublishedToDate;
		private bool enabled;

		public bool HasApplicationFunctionPermission(string applicationFunctionPath)
		{
			if (!enabled) return true;
			return _applicationFunctions.ContainsKey(applicationFunctionPath);
		}

		public bool HasPersonPermission(string applicationFunctionPath, DateOnly date, IPerson person)
		{
			if (!enabled) return true;
			return
				_personPermissionDatas.Any(
					x => x.ApplicationFunctionPath == applicationFunctionPath && x.Date == date && x.Person == person);
		}

		public bool HasTeamPermission(string applicationFunctionPath, DateOnly date, ITeam team)
		{
			if (!enabled) return true;
			return
				_teamPermissionData.Any(
					x => x.ApplicationFunctionPath == applicationFunctionPath && x.Date == date && x.Team == team);
		}

		public bool HasSitePermission(string applicationfunctionpath, DateOnly today, ISite site)
		{
			if (!enabled) return true;
			return
				_sitePermissionData.Any(
					x => x.ApplicationFunctionPath == applicationfunctionpath && x.Date == today && x.Site == site);
		}

		public bool HasOrganisationDetailPermission(string applicationFunctionPath, DateOnly date,
			IPersonAuthorizationInfo personAuthorizationInfo)
		{
			if (!enabled) return true;
			return _applicationFunctions.ContainsKey(applicationFunctionPath)
				&& (!_applicationFunctions[applicationFunctionPath].HasValue || _applicationFunctions[applicationFunctionPath] <= date);
		}

		public bool IsPersonSchedulePublished(DateOnly date, IPerson person,
			ScheduleVisibleReasons reason = ScheduleVisibleReasons.Published)
		{
			return !_schedulePublishedToDate.HasValue || _schedulePublishedToDate.Value >= date;
		}

		public void Enable()
		{
			enabled = true;
		}

		public void Disable()
		{
			enabled = false;
		}

		public void Permit(string applicationFunction)
		{
			if (!_applicationFunctions.ContainsKey(applicationFunction))
				_applicationFunctions.Add(applicationFunction, null);
		}

		public void Permit(string applicationFunction, DateOnly date)
		{
			if (!_applicationFunctions.ContainsKey(applicationFunction))
				_applicationFunctions.Add(applicationFunction, date);
		}

		public void Reject(string applicationFunction)
		{
			if (_applicationFunctions.ContainsKey(applicationFunction))
				_applicationFunctions.Remove(applicationFunction);
		}

		public void PermitPerson(string applicationFunction,  IPerson person, DateOnly date)
		{
			_personPermissionDatas.Add(new PersonPermissionData(applicationFunction, date, person));
		}

		public void PublishToDate(DateOnly date)
		{
			_schedulePublishedToDate = date;
		}

		public void PermitSite(string applicationFunction, ISite site, DateOnly date)
		{
			_sitePermissionData.Add(new SitePermissionData(applicationFunction, date, site));
		}

		public void PermitTeam(string applicationFunction, ITeam team, DateOnly date)
		{
			_teamPermissionData.Add(new TeamPermissionData(applicationFunction, date, team));
		}
	}

	class PersonPermissionData
	{
		public string ApplicationFunctionPath;
		public DateOnly Date;
		public IPerson Person;

		public PersonPermissionData(string applicationFunctionPath, DateOnly date, IPerson person)
		{
			ApplicationFunctionPath = applicationFunctionPath;
			Date = date;
			Person = person;
		}
	}

	class SitePermissionData
	{
		public string ApplicationFunctionPath;
		public DateOnly Date;
		public ISite Site;

		public SitePermissionData(string applicationFunctionPath, DateOnly date, ISite site)
		{
			ApplicationFunctionPath = applicationFunctionPath;
			Date = date;
			Site = site;
		}
	}
	class TeamPermissionData
	{
		public string ApplicationFunctionPath;
		public DateOnly Date;
		public ITeam Team;

		public TeamPermissionData(string applicationFunctionPath, DateOnly date, ITeam team)
		{
			ApplicationFunctionPath = applicationFunctionPath;
			Date = date;
			Team = team;
		}
	}
}