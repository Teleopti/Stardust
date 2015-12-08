namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface IAbsenceCommandConverter
	{
		AbsenceCreatorInfo GetCreatorInfo(AddFullDayAbsenceCommand command);
	}
}
