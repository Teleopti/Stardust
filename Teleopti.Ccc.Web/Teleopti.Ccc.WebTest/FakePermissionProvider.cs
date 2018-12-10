using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.WebTest
{
	public class FakePermissionProvider : IPermissionProvider
	{
		private bool _canSeeUnpublishedSchedule;
		private readonly IList<PersonPermissionData> _personPermissionDatas = new List<PersonPermissionData>();
		private Dictionary<string, bool> _applicationFunctionPermission = new Dictionary<string, bool>();

		public FakePermissionProvider()
		{
			_canSeeUnpublishedSchedule = true;
		}

		public FakePermissionProvider(bool canSeeUnpublishedSchedule)
		{
			_canSeeUnpublishedSchedule = canSeeUnpublishedSchedule;
		}

		public void PermitPerson(string applicationFunctionPath, DateOnly date, IPerson person)
		{
			_personPermissionDatas.Add(new PersonPermissionData(applicationFunctionPath, date, person));
		}

		public void SetApplicationFunctionPermission(string applicationFunctionPath, bool hasPermission)
		{
			_applicationFunctionPermission.Add(applicationFunctionPath, hasPermission);
		}

		public bool HasApplicationFunctionPermission(string applicationFunctionPath)
		{
			if (_applicationFunctionPermission.ContainsKey(applicationFunctionPath))
			{
				_applicationFunctionPermission.TryGetValue(applicationFunctionPath, out var hasPermission);
				return hasPermission;
			}

			return _canSeeUnpublishedSchedule || applicationFunctionPath != DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules;

		}

		public bool HasPersonPermission(string applicationFunctionPath, DateOnly date, IPerson person)
		{
			return
				_personPermissionDatas.Any(
					data => data.Person.Id == person.Id 
							&& data.ApplicationFunctionPath == applicationFunctionPath 
							&& data.Date == date);
		}

		public bool HasTeamPermission(string applicationFunctionPath, DateOnly date, ITeam team)
		{
			return true;
		}

		public bool HasSitePermission(string applicationfunctionpath, DateOnly today, ISite site)
		{
			return true;
		}

		public bool HasOrganisationDetailPermission(string applicationFunctionPath, DateOnly date, IPersonAuthorization personAuthorizationInfo)
		{
			return true;
		}

		public bool IsPersonSchedulePublished(DateOnly date, IPerson person, ScheduleVisibleReasons reason)
		{
			var name = person.Name.FirstName + person.Name.LastName;
			if (date < new DateOnly(2015, 1, 1)) return true; // to avoid invalid date
			if (name.Contains("Unpublish")) return false;
			return true;

		}
		public void PermitApplicationFunction(string applicationFunctionPath, bool value)
		{
			if (applicationFunctionPath == DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules)
			{
				_canSeeUnpublishedSchedule = value;
			}
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