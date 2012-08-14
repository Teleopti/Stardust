using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class CommonPartTimePercentage : IDataSetup
	{
		public IPartTimePercentage PartTimePercentage { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			PartTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage("Common PartTimePercentage");
			var repository = new Repository(uow);
			repository.Add(PartTimePercentage);
		}

	}
}