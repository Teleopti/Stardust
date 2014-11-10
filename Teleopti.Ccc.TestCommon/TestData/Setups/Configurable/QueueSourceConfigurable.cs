using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class QueueSourceConfigurable : IDataSetup
	{
		public string Name { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			var qs = new QueueSource {Name = Name};
			new QueueSourceRepository(uow).Add(qs);
		}
	}
}