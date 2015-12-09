namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddIntradayAbsenceCommandHandler : IHandleCommand<AddIntradayAbsenceCommand>
	{
		private readonly IPersonAbsenceCreator _personAbsenceCreator;
		private readonly IAbsenceCommandConverter _absenceCommandConverter;

		public AddIntradayAbsenceCommandHandler(IPersonAbsenceCreator personAbsenceCreator, IAbsenceCommandConverter absenceCommandConverter)
		{
			_personAbsenceCreator = personAbsenceCreator;
			_absenceCommandConverter = absenceCommandConverter;
		}
		
		public void Handle(AddIntradayAbsenceCommand command)
		{
			var absenceCreatorInfo = _absenceCommandConverter.GetCreatorInfoForIntradayAbsence(command);
			_personAbsenceCreator.Create(absenceCreatorInfo, false);
		}
	}
}