using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class BusinessUnitConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public IBusinessUnit BusinessUnit { get; set; }
		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var businessUnitRepository = new BusinessUnitRepository(currentUnitOfWork, null, null);
			BusinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit(Name);
			businessUnitRepository.Add(BusinessUnit);
		}
	}
}