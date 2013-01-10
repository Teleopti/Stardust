using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public interface IPossibleShiftTradePersonsProvider
	{
		IEnumerable<IPerson> RetrievePersons();
	}

	public class PossibleShiftTradePersonsProvider : IPossibleShiftTradePersonsProvider
	{
		public IEnumerable<IPerson> RetrievePersons()
		{
			return new List<IPerson>();
		}
	}
}