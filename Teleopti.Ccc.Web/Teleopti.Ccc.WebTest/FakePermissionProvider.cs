using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest
{
	public class FakePermissionProvider : IPermissionProvider
	{

		private readonly bool _canSeeUnpublishedSchedule;

		public FakePermissionProvider()
		{
			_canSeeUnpublishedSchedule = true;
		} 

		public FakePermissionProvider(bool canSeeUnpublishedSchedule)
		{
			_canSeeUnpublishedSchedule = canSeeUnpublishedSchedule;
		}


		public bool HasApplicationFunctionPermission(string applicationFunctionPath)
		{

			return _canSeeUnpublishedSchedule ||
			       applicationFunctionPath != DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules;
			
		}

		public bool HasPersonPermission(string applicationFunctionPath, DateOnly date, IPerson person)
		{
			return true;
		}

		public bool HasTeamPermission(string applicationFunctionPath, DateOnly date, ITeam team)
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
			if (date < new DateOnly(2015, 1, 1)) return false; // to avoid invalid date
			if (name.Contains("Unpublish")) return false;
			return true;
						
		}
	}
}