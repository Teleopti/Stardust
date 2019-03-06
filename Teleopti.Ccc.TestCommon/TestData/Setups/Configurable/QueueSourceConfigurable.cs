using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class QueueSourceConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public int QueueId { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var qs = new QueueSource { Name = Name, QueueMartId = QueueId };
			QueueSourceRepository.DONT_USE_CTOR(currentUnitOfWork).Add(qs);
		}

	}
}