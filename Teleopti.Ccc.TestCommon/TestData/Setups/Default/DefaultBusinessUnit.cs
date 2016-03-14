using log4net;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Default
{
	public class DefaultBusinessUnit : IHashableDataSetup
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(DefaultBusinessUnit));

		public static IBusinessUnit BusinessUnit = new BusinessUnit("BusinessUnit");

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var businessUnitRepository = new BusinessUnitRepository(currentUnitOfWork);
			businessUnitRepository.Add(BusinessUnit);
		}

		public int HashValue()
		{
			log.Debug("BusinessUnit.Name.GetHashCode() " + BusinessUnit.Name.GetHashCode());
			return BusinessUnit.Name.GetHashCode();
		}
	}
}