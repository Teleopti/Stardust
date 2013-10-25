using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class SecondBusinessUnit : IDataSetup
	{
		public IBusinessUnit BusinessUnit;

		public void Apply(IUnitOfWork uow)
		{
			BusinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("SecondBusinessUnit");
			var businessUnitRepository = new BusinessUnitRepository(uow);
			businessUnitRepository.Add(BusinessUnit);
		}
	}
}