using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IMatrixListFactory
	{
		IList<IScheduleMatrixPro> CreateMatrixList(IList<IScheduleDay> scheduleDays, DateOnlyPeriod selectedPeriod);
		IList<IScheduleMatrixPro> CreateMatrixListAllForLoadedPeriod(DateOnlyPeriod selectedPeriod);
	}
}