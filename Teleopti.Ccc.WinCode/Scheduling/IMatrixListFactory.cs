using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IMatrixListFactory
	{
		IList<IScheduleMatrixPro> CreateMatrixListAll(DateOnlyPeriod selectedPeriod);
		IList<IScheduleMatrixPro> CreateMatrixList(IList<IScheduleDay> scheduleDays, DateOnlyPeriod selectedPeriod);
	}
}