using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public interface IShiftTradePersonScheduleProvider
	{
		IEnumerable<IScheduleDay> GetScheduleForPersons(DateOnly date, IEnumerable<IPerson> persons);
	}
}