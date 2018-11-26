using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Adherence.Domain.ApprovePeriodAsInAdherence
{
	public class ApprovePeriodAsInAdherenceCommandHandler
	{
		private readonly ApprovePeriodAsInAdherence _approve;
		private readonly IUserTimeZone _timeZone;
		private readonly ICurrentAuthorization _authorization;
		private readonly IPersonRepository _persons;

		public ApprovePeriodAsInAdherenceCommandHandler(
			ApprovePeriodAsInAdherence approve,
			IUserTimeZone timeZone,
			ICurrentAuthorization authorization,
			IPersonRepository persons)
		{
			_approve = approve;
			_timeZone = timeZone;
			_authorization = authorization;
			_persons = persons;
		}

		public void Handle(ApprovePeriodAsInAdherenceCommand command)
		{
			// should really be in ApprovePeriodAsInAdherence using the correct date?
			var date = DateTime.ParseExact(command.StartDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
			if (!_authorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyAdherence, date, _persons.Load(command.PersonId)))
				throw new PermissionException();

			var startDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(command.StartDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), _timeZone.TimeZone());
			var endDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(command.EndDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), _timeZone.TimeZone());
			_approve.Approve(new ApprovedPeriod
			{
				PersonId = command.PersonId,
				StartTime = startDateTime,
				EndTime = endDateTime,
			});
		}
	}
}