using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory
{
	public interface IBusinessUnitsViewModelFactory
	{
		IEnumerable<BusinessUnitViewModel> BusinessUnits(IDataSource dataSource, IPerson person);
	}
}