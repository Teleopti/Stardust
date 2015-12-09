namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface IPersonAbsenceCreator
	{
		object Create(AbsenceCreatorInfo info, bool isFullDayAbsence, bool isNeedRuleCheckResult = false);
	}
}