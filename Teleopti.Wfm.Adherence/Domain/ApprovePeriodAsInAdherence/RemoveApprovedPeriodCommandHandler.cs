using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Wfm.Adherence.Domain.ApprovePeriodAsInAdherence
{
	public class RemoveApprovedPeriodCommandHandler
	{
		private readonly RemoveApprovedPeriod _remove;
		private readonly IUserTimeZone _timeZone;
		private readonly ICurrentAuthorization _authorization;
		private readonly IPersonRepository _persons;

		public RemoveApprovedPeriodCommandHandler(
			RemoveApprovedPeriod remove,
			IUserTimeZone timeZone,
			ICurrentAuthorization authorization,
			IPersonRepository persons)
		{
			_remove = remove;
			_timeZone = timeZone;
			_authorization = authorization;
			_persons = persons;
		}

		public void Handle(RemoveApprovedPeriodCommand command)
		{
			// should really be in RemoveApprovedPeriod using the correct date?
			var date = DateTime.ParseExact(command.StartDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
			if (!_authorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyAdherence, date, _persons.Load(command.PersonId)))
				throw new PermissionException();

			var startDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(command.StartDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), _timeZone.TimeZone());
			var endDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(command.EndDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), _timeZone.TimeZone());
			_remove.Remove(new RemovedPeriod
			{
				PersonId = command.PersonId,
				StartTime = startDateTime,
				EndTime = endDateTime
			});
		}
	}
}