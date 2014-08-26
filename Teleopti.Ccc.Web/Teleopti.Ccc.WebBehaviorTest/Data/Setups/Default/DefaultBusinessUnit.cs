using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default
{
	public class DefaultBusinessUnit : IHashableDataSetup
	{
		public readonly static IBusinessUnit BusinessUnitFromFakeState = new BusinessUnit("BusinessUnit");

		public IBusinessUnit BusinessUnit { get { return BusinessUnitFromFakeState; } }

		public void Apply(IUnitOfWork uow)
		{
			var businessUnitRepository = new BusinessUnitRepository(uow);
			businessUnitRepository.Add(BusinessUnit);
		}

		public int HashValue()
		{
			return BusinessUnit.Name.GetHashCode();
		}
	}
}