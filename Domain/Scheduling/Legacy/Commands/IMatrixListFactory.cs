using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IMatrixListFactory
	{
		IList<IScheduleMatrixPro> CreateMatrixListAllForLoadedPeriod(IScheduleDictionary schedules, IEnumerable<IPerson> personsInOrganization, DateOnlyPeriod selectedPeriod);
		IList<IScheduleMatrixPro> CreateMatrixListForSelection(IScheduleDictionary schedules, IEnumerable<IScheduleDay> scheduleDays);
		IEnumerable<IScheduleMatrixPro> CreateMatrixListForSelection(IScheduleDictionary schedules, IEnumerable<IPerson> agents, DateOnlyPeriod selectedPeriod);
	}
}