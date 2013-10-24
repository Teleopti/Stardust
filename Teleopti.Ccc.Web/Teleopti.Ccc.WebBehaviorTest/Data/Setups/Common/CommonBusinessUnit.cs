using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common
{
	public class CommonBusinessUnit : IDataSetup
	{
		public static IBusinessUnit BusinessUnitFromFakeState;

		public IBusinessUnit BusinessUnit { get { return BusinessUnitFromFakeState; } }

		public void Apply(IUnitOfWork uow)
		{
			var businessUnitRepository = new BusinessUnitRepository(uow);
			businessUnitRepository.Add(BusinessUnit);
		}
	}
}