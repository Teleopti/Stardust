using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Default
{
	public class DefaultBusinessUnit : IHashableDataSetup
	{
		public static IBusinessUnit BusinessUnitFromFakeState = new BusinessUnit("BusinessUnit");

		public IBusinessUnit BusinessUnit { get { return BusinessUnitFromFakeState; } }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var businessUnitRepository = new BusinessUnitRepository(currentUnitOfWork);
			businessUnitRepository.Add(BusinessUnit);
		}

		public int HashValue()
		{
			return BusinessUnit.Name.GetHashCode();
		}
	}
}