using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory
{
	public class BusinessUnitsViewModelFactory : IBusinessUnitsViewModelFactory
	{
		private readonly IBusinessUnitProvider _businessUnitProvider;

		public BusinessUnitsViewModelFactory(IBusinessUnitProvider businessUnitProvider)
		{
			_businessUnitProvider = businessUnitProvider;
		}

		public IEnumerable<BusinessUnitViewModel> BusinessUnits(IDataSource dataSource, IPerson person)
		{
			var businessUnits = _businessUnitProvider.RetrieveBusinessUnitsForPerson(dataSource, person);
			return (
				       from b in businessUnits
				       select new BusinessUnitViewModel
					       {
						       Id = b.Id.Value,
						       Name = b.Name
					       })
				.ToList();
		}

	}
}