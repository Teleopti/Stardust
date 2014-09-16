using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class BusinessUnitConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public IBusinessUnit BusinessUnit { get; set; }
		public void Apply(IUnitOfWork uow)
		{
			var businessUnitRepository = new BusinessUnitRepository(uow);
			BusinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit(Name);
			businessUnitRepository.Add(BusinessUnit);
		}
	}
}