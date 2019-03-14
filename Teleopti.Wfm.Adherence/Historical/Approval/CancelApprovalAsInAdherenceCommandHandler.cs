using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Wfm.Adherence.Historical.Approval
{
	public class CancelApprovalAsInAdherenceCommandHandler
	{
		private readonly Historical.Approval.Approval _approval;
		private readonly IUserTimeZone _timeZone;
		private readonly ICurrentAuthorization _authorization;
		private readonly IPersonRepository _persons;

		public CancelApprovalAsInAdherenceCommandHandler(
			Historical.Approval.Approval approval,
			IUserTimeZone timeZone,
			ICurrentAuthorization authorization,
			IPersonRepository persons)
		{
			_approval = approval;
			_timeZone = timeZone;
			_authorization = authorization;
			_persons = persons;
		}

		public void Handle(CancelApprovalAsInAdherenceCommand approvalAsInAdherenceCommand)
		{
			// should really be in RemoveApprovedPeriod using the correct date?
			var date = DateTime.ParseExact(approvalAsInAdherenceCommand.StartDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
			if (!_authorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyAdherence, new Ccc.Domain.InterfaceLegacy.Domain.DateOnly(date), _persons.Load(approvalAsInAdherenceCommand.PersonId)))
				throw new PermissionException();

			var startDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(approvalAsInAdherenceCommand.StartDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), _timeZone.TimeZone());
			var endDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(approvalAsInAdherenceCommand.EndDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), _timeZone.TimeZone());
			_approval.Cancel(new PeriodToCancel
			{
				PersonId = approvalAsInAdherenceCommand.PersonId,
				StartTime = startDateTime,
				EndTime = endDateTime
			});
		}
	}
}