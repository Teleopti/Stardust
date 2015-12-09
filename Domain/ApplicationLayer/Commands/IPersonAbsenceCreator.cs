using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface IPersonAbsenceCreator
	{
		PersonAbsence Create (AbsenceCreatorInfo info, bool isFullDayAbsence);
	}
}