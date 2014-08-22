using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PartTimePercentageConfigurable : IDataSetup
	{
		public string Name { get; set; }

		public IPartTimePercentage PartTimePercentage { get; private set; }

		public void Apply(IUnitOfWork uow)
		{
			if (Name == null)
			{
				Name = DefaultName.Make();
			}
			PartTimePercentage = new PartTimePercentage(Name);
			var repository = new Repository(uow);
			repository.Add(PartTimePercentage);
		}

	}
}