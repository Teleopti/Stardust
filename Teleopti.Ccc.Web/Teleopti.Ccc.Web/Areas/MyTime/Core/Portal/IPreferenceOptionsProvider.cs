using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal
{
	public interface IPreferenceOptionsProvider
	{
		IEnumerable<IShiftCategory> RetrieveShiftCategoryOptions();
		IEnumerable<IDayOffTemplate> RetrieveDayOffOptions();
		IEnumerable<IAbsence> RetrieveAbsenceOptions();
	}
}