using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class FakePermissionProvider : IPermissionProvider
	{
		private readonly Dictionary<string, DateOnly?> _applicationFunctions = new Dictionary<string, DateOnly?>();
		private readonly IList<PersonPermissionData> _personPermissionDatas = new List<PersonPermissionData>();
		private DateOnly? _schedulePublishedToDate;
		private bool enabled;

		public bool HasApplicationFunctionPermission(string applicationFunctionPath)
		{
			if (!enabled) return true;
			return _applicationFunctions.ContainsKey(applicationFunctionPath);
		}

		public bool HasPersonPermission(string applicationFunctionPath, DateOnly date, IPerson person)
		{
			throw new NotImplementedException();
		}

		public bool HasTeamPermission(string applicationFunctionPath, DateOnly date, ITeam team)
		{
			throw new NotImplementedException();
		}

		public bool HasSitePermission(string applicationfunctionpath, DateOnly today, ISite site)
		{
			throw new NotImplementedException();
		}

		public bool HasOrganisationDetailPermission(string applicationFunctionPath, DateOnly date,
			IAuthorizeOrganisationDetail authorizeOrganisationDetail)
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
}