using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
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