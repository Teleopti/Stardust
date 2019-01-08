using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface IPersonAbsenceCreator
	{
		IList<string> Create(AbsenceCreatorInfo info, bool isFullDayAbsence, bool muteEvent = false);
	}
}