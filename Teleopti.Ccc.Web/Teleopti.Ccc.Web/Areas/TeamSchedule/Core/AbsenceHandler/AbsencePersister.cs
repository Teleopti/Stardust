using Teleopti.Ccc.Domain.ApplicationLayer.Commands;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.AbsenceHandler
{
	public class AbsencePersister :IAbsencePersister
	{
		private readonly IAbsenceCommandConverter _absenceCommandConverter;
		private readonly IPersonAbsenceCreator _personAbsenceCreator;

		public AbsencePersister(IAbsenceCommandConverter absenceCommandConverter, IPersonAbsenceCreator personAbsenceCreator)
		{
			_absenceCommandConverter = absenceCommandConverter;
			_personAbsenceCreator = personAbsenceCreator;
		}

		public void PersistFullDayAbsence(AddFullDayAbsenceCommand command)
		{
			var absenceCreatorInfo = _absenceCommandConverter.GetCreatorInfo(command);
			_personAbsenceCreator.Create(absenceCreatorInfo.Absence, absenceCreatorInfo.ScheduleRange, absenceCreatorInfo.ScheduleDay, absenceCreatorInfo.AbsenceTimePeriod, absenceCreatorInfo.Person, command.TrackedCommandInfo, true);
		}

	}
}