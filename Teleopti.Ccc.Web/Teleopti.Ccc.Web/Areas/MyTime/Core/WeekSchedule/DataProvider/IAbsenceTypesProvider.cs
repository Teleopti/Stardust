using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider
{
	public interface IAbsenceTypesProvider
	{
		IEnumerable<IAbsence> GetRequestableAbsences();
		IEnumerable<IAbsence> GetReportableAbsences();
	}
}