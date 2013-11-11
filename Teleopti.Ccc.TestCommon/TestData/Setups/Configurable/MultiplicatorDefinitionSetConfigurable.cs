using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class MultiplicatorDefinitionSetConfigurable : IDataSetup
	{
		public string Name { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			var multiplicatorDefinitionSetRepository = new MultiplicatorDefinitionSetRepository(uow);
			multiplicatorDefinitionSetRepository.Add(new MultiplicatorDefinitionSet(Name, MultiplicatorType.Overtime));
		}
	}
}