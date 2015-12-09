namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface IAbsenceCommandConverter
	{
		AbsenceCreatorInfo GetCreatorInfoForFullDayAbsence(AddFullDayAbsenceCommand command);
		AbsenceCreatorInfo GetCreatorInfoForIntradayAbsence(AddIntradayAbsenceCommand command);
	}
}
