using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Default
{
	public class DefaultBusinessUnit : IHashableDataSetup
	{
		public static IBusinessUnit BusinessUnitFromFakeState = new BusinessUnit("BusinessUnit");

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var businessUnitRepository = new BusinessUnitRepository(currentUnitOfWork);
			businessUnitRepository.Add(BusinessUnitFromFakeState);
		}

		public int HashValue()
		{
			return BusinessUnitFromFakeState.Name.GetHashCode();
		}
	}
}