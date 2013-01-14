using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public interface IPossibleShiftTradePersonsProvider
	{
		IEnumerable<IPerson> RetrievePersons(DateOnly dateOnly);
	}
}