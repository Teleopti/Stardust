using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;

namespace Teleopti.Ccc.Domain.Security
{
	public class PermissionProvider : IPermissionProvider
	{
		private readonly IAuthorization _authorization;

		public PermissionProvider(IAuthorization authorization)
		{
			_authorization = authorization;
		}

		public bool HasApplicationFunctionPermission(string applicationFunctionPath)
		{
			return _authorization.IsPermitted(applicationFunctionPath);
		}

		public bool HasPersonPermission(string applicationFunctionPath, DateOnly date, IPerson person)
		{
			return _authorization.IsPermitted(applicationFunctionPath, date, person);
		}

		public bool HasTeamPermission(string applicationFunctionPath, DateOnly date, ITeam team)
		{
			return _authorization.IsPermitted(applicationFunctionPath, date, team);
		}

		public bool HasOrganisationDetailPermission(string applicationFunctionPath, DateOnly date, IPersonAuthorization personAuthorization)
		{
			return _authorization.IsPermitted(applicationFunctionPath, date, personAuthorization);
		}

		public bool IsPersonSchedulePublished(DateOnly date, IPerson person, ScheduleVisibleReasons reason = ScheduleVisibleReasons.Published)
		{
			var dayAndPeriod = new DateOnlyAsDateTimePeriod(date, person.PermissionInformation.DefaultTimeZone());
			var schedulePublishedSpecification = new SchedulePublishedSpecification(person.WorkflowControlSet, reason);
			var schedIsPublished = schedulePublishedSpecification.IsSatisfiedBy(dayAndPeriod.DateOnly);
			return schedIsPublished;
		}

		public bool HasSitePermission(string applicationfunctionpath, DateOnly today, ISite site)
		{
			return _authorization.IsPermitted(applicationfunctionpath, today, site);
		}
	}
}
