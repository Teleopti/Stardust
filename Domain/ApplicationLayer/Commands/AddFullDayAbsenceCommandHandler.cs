namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddFullDayAbsenceCommandHandler : IHandleCommand<AddFullDayAbsenceCommand>
	{
		private readonly IPersonAbsenceCreator _personAbsenceCreator;
		private readonly IAbsenceCommandConverter _absenceCommandConverter;
		
		public AddFullDayAbsenceCommandHandler(IPersonAbsenceCreator personAbsenceCreator, IAbsenceCommandConverter absenceCommandConverter)
		{
			_personAbsenceCreator = personAbsenceCreator;
			_absenceCommandConverter = absenceCommandConverter;
		}

		public void Handle(AddFullDayAbsenceCommand command)
		{
			var absenceCreatorInfo = _absenceCommandConverter.GetCreatorInfo(command);
			_personAbsenceCreator.Create(absenceCreatorInfo.Absence, absenceCreatorInfo.ScheduleRange, absenceCreatorInfo.ScheduleDay, absenceCreatorInfo.AbsenceTimePeriod, absenceCreatorInfo.Person, command.TrackedCommandInfo, true);
		}
	}
}