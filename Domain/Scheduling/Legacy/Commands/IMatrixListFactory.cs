using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IMatrixListFactory
	{
		IList<IScheduleMatrixPro> CreateMatrixListAllForLoadedPeriod(DateOnlyPeriod selectedPeriod);
		IList<IScheduleMatrixPro> CreateMatrixListForSelection(IList<IScheduleDay> scheduleDays);
		IList<IScheduleMatrixPro> CreateMatrixListForSelectionPerPerson(IList<IScheduleDay> scheduleDays);
	}
}