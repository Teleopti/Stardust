using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider
{
	public class PermissionProvider : IPermissionProvider
	{
		private readonly IPrincipalAuthorization _principalAuthorization;

		public PermissionProvider(IPrincipalAuthorization principalAuthorization)
		{
			_principalAuthorization = principalAuthorization;
		}

		public bool HasApplicationFunctionPermission(string applicationFunctionPath)
		{
			return _principalAuthorization.IsPermitted(applicationFunctionPath);
		}

		public bool HasPersonPermission(string applicationFunctionPath, DateOnly date, IPerson person)
		{
			return _principalAuthorization.IsPermitted(applicationFunctionPath, date, person);
		}

		public bool HasTeamPermission(string applicationFunctionPath, DateOnly date, ITeam team)
		{
			return _principalAuthorization.IsPermitted(applicationFunctionPath, date, team);
		}

		public bool HasOrganisationDetailPermission(string applicationFunctionPath, DateOnly date, IAuthorizeOrganisationDetail authorizeOrganisationDetail)
		{
			return _principalAuthorization.IsPermitted(applicationFunctionPath, date, authorizeOrganisationDetail);
		}

		public bool IsPersonSchedulePublished(DateOnly date,
			IPerson person)
		{
			var dayAndPeriod = new DateOnlyAsDateTimePeriod(date,
				person.PermissionInformation.DefaultTimeZone());
			var schedulePublishedSpecification = new SchedulePublishedSpecification(person.WorkflowControlSet,
				ScheduleVisibleReasons.Published);
			var schedIsPublished = schedulePublishedSpecification.IsSatisfiedBy(dayAndPeriod.DateOnly);
			return schedIsPublished;
		}
	}
}