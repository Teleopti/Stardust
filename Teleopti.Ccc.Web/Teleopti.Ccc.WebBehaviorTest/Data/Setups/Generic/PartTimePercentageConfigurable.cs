using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class PartTimePercentageConfigurable : IDataSetup
	{
		public string Name { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			var partTimePercentage = new PartTimePercentage(Name);
			var repository = new Repository(uow);
			repository.Add(partTimePercentage);
		}

	}
}