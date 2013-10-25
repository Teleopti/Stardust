using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common
{
	public class CommonPartTimePercentage : IDataSetup
	{
		public IPartTimePercentage PartTimePercentage { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			PartTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage(DefaultName.Make("Common PartTimePercentage"));
			var repository = new Repository(uow);
			repository.Add(PartTimePercentage);
		}

	}
}