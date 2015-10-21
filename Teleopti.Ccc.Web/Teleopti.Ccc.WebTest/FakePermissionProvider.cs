using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest
{
	public class FakePermissionProvider : IPermissionProvider
	{
		private readonly bool _canSeeUnpublishedSchedule;
		private readonly IList<PersonPermissionData> _personPermissionDatas = new List<PersonPermissionData>();

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

		public bool HasApplicationFunctionPermission(string applicationFunctionPath)
		{

			return _canSeeUnpublishedSchedule ||
				   applicationFunctionPath != DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules;

		}

		public bool HasPersonPermission(string applicationFunctionPath, DateOnly date, IPerson person)
		{
			return
				_personPermissionDatas.Any(
					data => data.Person.Id == person.Id 
							&& data.ApplicationFunctionPath == applicationFunctionPath 
							&& data.Date == date);
		}

		public bool HasNoPersonPermission(string applicationFunctionPath, DateOnly date, IPerson person)
		{
			return false;
		}

		public bool HasTeamPermission(string applicationFunctionPath, DateOnly date, ITeam team)
		{
			return true;
		}

		public bool HasSitePermission(string applicationfunctionpath, DateOnly today, ISite site)
		{
			return true;
		}

		public bool HasOrganisationDetailPermission(string applicationFunctionPath, DateOnly date,
			IAuthorizeOrganisationDetail authorizeOrganisationDetail)
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