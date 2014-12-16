using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{

	public class ModifyPersonAbsenceCommandHandler : IHandleCommand<ModifyPersonAbsenceCommand>
	{

		private readonly IWriteSideRepositoryTypedId<IPersonAbsence, Guid> _personAbsenceRepository;
		private IUserTimeZone _timeZone;

		public ModifyPersonAbsenceCommandHandler(IWriteSideRepositoryTypedId<IPersonAbsence, Guid> personAbsenceRepository, IUserTimeZone timeZone)
		{
			_personAbsenceRepository = personAbsenceRepository;
			_timeZone = timeZone;
		}

		public void Handle(ModifyPersonAbsenceCommand command)
		{
			var personAbsence = (PersonAbsence)_personAbsenceRepository.LoadAggregate(command.PersonAbsenceId);

			if (personAbsence == null)
				throw new InvalidOperationException(string.Format("Person Absence is not found. StartTime: {0} EndTime: {1} PersonId: {2} AbsenceId: {3}  ", command.StartTime, command.EndTime, command.PersonId,command.PersonAbsenceId));


			var absenceTimePeriod = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(command.StartTime, _timeZone.TimeZone()),
													   TimeZoneHelper.ConvertToUtc(command.EndTime, _timeZone.TimeZone()));

			personAbsence.ModifyPersonAbsencePeriod(absenceTimePeriod, command.TrackedCommandInfo);
			
			
		}
	}
}
