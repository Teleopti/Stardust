using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider
{
	public interface IBusinessUnitProvider
	{
		IEnumerable<IBusinessUnit> RetrieveBusinessUnitsForPerson(IDataSource dataSource, IPerson person);
	}
}