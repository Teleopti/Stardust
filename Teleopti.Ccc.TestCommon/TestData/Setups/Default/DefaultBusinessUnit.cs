using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Default
{
	public class DefaultBusinessUnit : IHashableDataSetup
	{
		public static IBusinessUnit BusinessUnit = new BusinessUnit("BusinessUnit");

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var businessUnitRepository = BusinessUnitRepository.DONT_USE_CTOR(currentUnitOfWork, null, null);
			businessUnitRepository.Add(BusinessUnit);
		}

		public int HashValue()
		{
			return BusinessUnit.Name.GetHashCode();
		}
	}
}